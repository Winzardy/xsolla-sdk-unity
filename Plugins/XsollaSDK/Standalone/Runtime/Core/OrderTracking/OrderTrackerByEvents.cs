using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xsolla.GetUpdates;

namespace Xsolla.Core
{
    internal static class OrderTrackerByEventsUpdater
    {
        private static readonly Dictionary<string, OrderTrackerByEventsUpdaterInstance> Updaters 
            = new Dictionary<string, OrderTrackerByEventsUpdaterInstance>();
        
        private static OrderTrackerByEventsUpdaterInstance GetUpdater(XsollaSettings settings)
        {
            if (!Updaters.ContainsKey(settings.StoreProjectId))
                return null;

            return Updaters[settings.StoreProjectId];
        }
        
        private static OrderTrackerByEventsUpdaterInstance GetOrCreateUpdater(XsollaSettings settings)
        {
            if (!Updaters.ContainsKey(settings.StoreProjectId))
                Updaters[settings.StoreProjectId] = new OrderTrackerByEventsUpdaterInstance(settings);

            return Updaters[settings.StoreProjectId];
        }
        
        public static void StartEndlessUpdater(XsollaSettings settings, int intervalSec, Action<PaymentEvent[]> onEvent)
        {
            var updater = GetOrCreateUpdater(settings);
            updater.StartEndlessUpdater(intervalSec, onEvent);
        }

        public static void StartUpdater(OrderTrackerByEvents tracker)
        {
            var updater = GetOrCreateUpdater(tracker.TrackingData.settings);
            updater.StartUpdater(tracker);
        }

        public static void StopUpdater(OrderTrackerByEvents tracker)
        {
            var updater = GetUpdater(tracker.TrackingData.settings);
            if (updater != null)
                updater.StopUpdater(tracker);
        }

        public static void RemoveAllFromTracking()
        {
            foreach (var updater in Updaters.Values)
                updater.RemoveAllFromTracking();
        }
        
        public static void RemoveAllFromTracking(XsollaSettings settings)
        {
            var updater = GetUpdater(settings);
            if (updater != null)
                updater.RemoveAllFromTracking();
        }
        
        class OrderTrackerByEventsUpdaterInstance
        {
            private Coroutine _checkStatusCoroutine = null;
            private readonly List<OrderTrackerByEvents> _trackers = new List<OrderTrackerByEvents>();

            /// If 0, then the short polling is not currently active.
            private float _shortPollingTimeLimitSecs = 0.0f;

            private bool _isCheckInProgress = false;

            /// If 0, then the endless mode is disabled.
            private float _endlessModeIntervalSecs = 0.0f;

            private float _waitIntervalSecs = Constants.EVENTS_POLLING_INTERVAL;
            private Action<PaymentEvent[]> _onUnorderedPurchaseEvent = null;

            private bool isEndlessEnabled => _endlessModeIntervalSecs > 0.0f;

            private bool isShortPollingActive => _shortPollingTimeLimitSecs > 0.0f;

            private XsollaSettings settings;

            public OrderTrackerByEventsUpdaterInstance(XsollaSettings settings)
            {
                this.settings = settings;
            }

            public void StartEndlessUpdater(int intervalSec, Action<PaymentEvent[]> onEvent)
            {
                _onUnorderedPurchaseEvent = onEvent;

                // Endless update interval shouldn't be shorter than the "accelerated" update interval.
                _endlessModeIntervalSecs = intervalSec > 0.0f
                    ? Math.Max(intervalSec, Constants.EVENTS_POLLING_INTERVAL)
                    : 0.0f;

                if (!isEndlessEnabled)
                {
                    XDebug.LogWarning(
                        settings,
                        "Unordered purchase events listener has been assigned, but the endless " +
                        $"mode is OFF due to the interval value provided ({intervalSec}). The listener " +
                        "will be invoked only when doing the short-polling."
                    );
                    return;
                }

                XDebug.Log(settings, $"[OrderTrackerByEvents] Starting endless updater (interval={intervalSec}s)");

                if (!isShortPollingActive)
                    ResetWaitInterval();

                StartUpdater();
            }

            public void StartUpdater()
            {
                if (_checkStatusCoroutine == null)
                {
                    _checkStatusCoroutine = CoroutinesExecutor.Run(TrackEvents());

                    XDebug.Log(settings, "[OrderTrackerByEvents] Started updater (" +
                                       $"interval={_waitIntervalSecs}s " +
                                       $"endless_enabled={isEndlessEnabled}" +
                                       ")"
                    );
                }
            }

            /// <summary>Stops the updater.</summary>
            /// <param name="switchToEndlessIfAvailable">
            /// If `true` the updater will attempt to switch into the endless mode
            /// or stopped completely otherwise (endless mode not available).
            /// </param>
            public void StopUpdater(bool switchToEndlessIfAvailable = true)
            {
                var wasShortPollingActive = isShortPollingActive;
                var switchToEndless = isEndlessEnabled && switchToEndlessIfAvailable;

                XDebug.Log(settings, "[OrderTrackerByEvents] Stopping updater (" +
                    $"switchToEndless={switchToEndless} " +
                    $"wasShortPollingActive={wasShortPollingActive}" +
                    ")"
                );

                _shortPollingTimeLimitSecs = 0.0f;

                ResetWaitInterval();

                if (switchToEndless)
                {
                    if (wasShortPollingActive)
                        RestartUpdater();
                    return;
                }

                KillUpdater();
            }

            public void StartUpdater(OrderTrackerByEvents tracker)
            {
                if (!_trackers.Contains(tracker))
                {
                    _trackers.Add(tracker);

                    var needUpdaterRestart = !isShortPollingActive;

                    XDebug.Log(settings, $"[OrderTrackerByEvents] Added a tracker (need_restart={needUpdaterRestart})");

                    ResetShortPollingTimeLimit();
                    ResetWaitInterval();

                    if (needUpdaterRestart)
                        RestartUpdater();
                }
            }

            public void StopUpdater(OrderTrackerByEvents tracker)
            {
                _trackers.Remove(tracker);

                if (_trackers.Count == 0)
                    StopUpdater();
            }

            public void RemoveAllFromTracking()
            {
                if (_trackers.Count > 0)
                {
                    var trackers = _trackers.ToArray();

                    _trackers.Clear();

                    StopUpdater();

                    foreach (var tracker in trackers)
                        tracker.ForceStop();
                }
            }

            private IEnumerator TrackEvents()
            {
                while (true)
                {
                    yield return new WaitForSecondsRealtime(_waitIntervalSecs);

                    UpdateEvents();

                    if (isShortPollingActive && Time.realtimeSinceStartup > _shortPollingTimeLimitSecs)
                    {
                        XDebug.Log(settings, $"[OrderTrackerByEvents] Short-polling timed out (endless_enabled={isEndlessEnabled})");

                        _shortPollingTimeLimitSecs = 0.0f;

                        ResetWaitInterval();

                        if (!isEndlessEnabled)
                        {
                            _checkStatusCoroutine = null;
                            XDebug.Log(settings, "[OrderTrackerByEvents] Completely stopped after short-polling (no endless mode enabled)");
                            HandleErrors(new Error(ErrorType.TimeLimitReached, errorMessage: "Polling time limit reached"));
                            yield break;
                        }

                        XDebug.Log(settings, "[OrderTrackerByEvents] Switched to endless mode after short-polling");
                    }
                }
            }

            private void UpdateEvents()
            {
                if (!settings.XsollaToken.Exists)
                {
                    XDebug.LogWarning(settings, "No Token in order status check. Check cancelled");
                    return;
                }

                if (_isCheckInProgress) // Prevent double check
                    return;

                _isCheckInProgress = true;

                XsollaGetUpdates.GetPaymentEvents(
                    settings,
                    onSuccess: events =>
                    {
                        _isCheckInProgress = false;
                        HandleEvents(events);
                    },
                    error =>
                    {
                        _isCheckInProgress = false;
                    },
                    markProcessed: false
                );
            }

            private void HandleEvents(PaymentEvent[] events)
            {
                if (events.Length == 0 || (_trackers.Count == 0 && _onUnorderedPurchaseEvent == null))
                    return;

                var trackersList = _trackers.ToArray();
                var remainingEvents = new List<PaymentEvent>(events);

                foreach (var tracker in trackersList)
                {
                    if (!tracker.CanUseTrackedOrder())
                        continue;

                    foreach (var evt in events)
                    {
                        if ((evt.order_id != -1 && evt.order_id == tracker.TrackingData.orderId) ||
                            (evt.order_id == -1 && evt.sku != null && evt.sku == tracker.TrackingData.sku))
                        {
                            tracker.HandleOrderDone(evt);
                            remainingEvents.Remove(evt);
                        }
                    }
                }

                if (remainingEvents.Count > 0)
                    _onUnorderedPurchaseEvent?.Invoke(remainingEvents.ToArray());
            }

            private void HandleErrors(Error error)
            {
                if (_trackers.Count == 0)
                    return;

                var trackersList = new List<OrderTrackerByEvents>(_trackers);

                foreach (var tracker in trackersList)
                    tracker.HandleError(error);
            }

            private void ResetShortPollingTimeLimit()
            {
                _shortPollingTimeLimitSecs = Time.realtimeSinceStartup + Constants.EVENTS_POLLING_LIMIT;

                XDebug.Log($"[OrderTrackerByEvents] ResetShortPollingTimeLimit: {_shortPollingTimeLimitSecs}s");
            }

            private void ResetWaitInterval()
            {
                _waitIntervalSecs = isShortPollingActive || !isEndlessEnabled
                    ? Constants.EVENTS_POLLING_INTERVAL
                    : _endlessModeIntervalSecs;

                XDebug.Log($"[OrderTrackerByEvents] ResetWaitInterval: {_waitIntervalSecs}s");
            }

            private void KillUpdater()
            {
                if (_checkStatusCoroutine != null)
                {
                    CoroutinesExecutor.Stop(_checkStatusCoroutine);
                    _checkStatusCoroutine = null;

                    XDebug.Log("[OrderTrackerByEvents] Killed updater");
                }
            }

            private void RestartUpdater()
            {
                KillUpdater();
                StartUpdater();
            }
        }
    }


    internal class OrderTrackerByEvents : OrderTracker
    {
        private OrderTrackerByPaystationCallbacksHelper _webHelper = null;

        public OrderTrackerByEvents(OrderTrackingData trackingData) : base(trackingData)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                _webHelper = new OrderTrackerByPaystationCallbacksHelper();
        }

        public override void Start()
        {
            _webHelper?.Start(
                onOk: () => { /* do nothing */ }, 
                onCancel: () => {
                    HandleError(new Error(ErrorType.UserCancelled));
                }
            );
            
            OrderTrackerByEventsUpdater.StartUpdater(this);
        }

        public override void Stop()
        {
            _webHelper?.Stop();
            
            OrderTrackerByEventsUpdater.StopUpdater(this);
        }

        public void ForceStop()
        {
            RemoveSelfFromTracking();
        }

        public void HandleOrderDone(PaymentEvent evt)
        {
            TrackingData?.successCallback?.Invoke(evt.ToOrderStatus());
            RemoveSelfFromTracking();
        }

        public void HandleError(Error error)
        {
            TrackingData?.errorCallback?.Invoke(error);
            RemoveSelfFromTracking();
        }

        public bool CanUseTrackedOrder() => CheckOrderIdExists();
    }
}
