using System;

namespace Xsolla.Core
{
	internal class OrderTrackerByPaystationCallbacksHelper
	{
		private bool isCancelByUser;
		private Action handleOK;
		private Action handleCancel;
		
		public void Start(Action onOk, Action onCancel)
		{
			handleOK = onOk;
			handleCancel = onCancel;
			
			XsollaWebCallbacks.AddPaymentStatusUpdateHandler(handleOK);
			XsollaWebCallbacks.AddPaymentCancelHandler(() =>
			{
				isCancelByUser = true;
				handleCancel?.Invoke();
			});
		}

		public void Stop()
		{
			XsollaWebCallbacks.RemovePaymentStatusUpdateHandler(handleOK);
			XsollaWebCallbacks.RemovePaymentCancelHandler(handleCancel);
			XsollaWebBrowserHandlerWebGL.ClosePayStation(isCancelByUser);
		}
	}
	
	internal class OrderTrackerByPaystationCallbacks : OrderTracker
	{
		private OrderTrackerByPaystationCallbacksHelper _helper = new OrderTrackerByPaystationCallbacksHelper();

		public OrderTrackerByPaystationCallbacks(OrderTrackingData trackingData) : base(trackingData) { }

		public override void Start()
		{
			_helper.Start(HandleStatusUpdate, HandlePaymentCancel);
		}

		public override void Stop()
		{
			_helper.Stop();
		}

		private void HandleStatusUpdate()
		{
			CheckOrderStatus(
				onDone: status =>
				{
					TrackingData?.successCallback?.Invoke(status);
					RemoveSelfFromTracking();
				},
				onCancel: () => RemoveSelfFromTracking(),
				error =>
				{
					TrackingData?.errorCallback?.Invoke(error);
					RemoveSelfFromTracking();
				}
			);
		}

		private void HandlePaymentCancel()
		{
			TrackingData?.errorCallback?.Invoke(new Error(ErrorType.UserCancelled));
			RemoveSelfFromTracking();
		}
	}
}
