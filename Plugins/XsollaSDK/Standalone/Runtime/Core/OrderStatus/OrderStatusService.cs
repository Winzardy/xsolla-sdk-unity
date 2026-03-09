using System;
using System.Collections.Generic;

namespace Xsolla.Core
{
	internal static class OrderStatusService
	{
		public static void GetOrderStatus(XsollaSettings settings, int orderId, Action<OrderStatus> onSuccess, Action<Error> onError, SdkType sdkType = SdkType.Store, string token = null)
		{
			if (OrderStatusCache.TryPerform(orderId, onSuccess))
				return;

			PerformWebRequest(
				settings,
				orderId,
				token,
				status =>
				{
					OrderStatusCache.UpdateStatus(status);
					onSuccess?.Invoke(status);
				},
				onError,
				sdkType);
		}

		private static void PerformWebRequest(XsollaSettings settings, int orderId, string token, Action<OrderStatus> onSuccess, Action<Error> onError, SdkType sdkType)
		{
			var url = $"https://store.xsolla.com/api/v2/project/{settings.StoreProjectId}/order/{orderId}";

			WebRequestHelper.Instance.GetRequest(
				sdkType,
				url,
				(!string.IsNullOrEmpty(token) ? WebRequestHeader.AuthHeader(token): WebRequestHeader.AuthHeader(settings)),
				onSuccess,
				error => TokenAutoRefresher.Check(settings, error, onError, () => PerformWebRequest(settings, orderId, token, onSuccess, onError, sdkType)),
				ErrorGroup.OrderStatusErrors);
		}
		
		public static void GetOrderId(XsollaSettings settings, string token, Action<int> onSuccess, Action<Error> onError, SdkType sdkType = SdkType.Store)
		{
            GetOrderInfo(settings, accessToken: token, sdkType,
				onSuccess: orderInfo => {
					if (orderInfo.TryAsDone(out var done)) {
						onSuccess((int)done.orderId);
					} 
					else 
					{
						onError.Invoke(new Error(errorMessage: $"Wrong post payment order status, expected 'done' but got '{orderInfo.GetType().Name}'"));
					}
				},
				onFailure: err => {
					onError.Invoke(err);
				}
			);
		}
		
		public static void GetOrderInfo(
            XsollaSettings settings,
            [System.Diagnostics.CodeAnalysis.NotNull]
            string accessToken, SdkType sdkType,
            Action<OrderInfo> onSuccess, Action<Error> onFailure
        )
        {
            if (string.IsNullOrEmpty(accessToken)) {
                onFailure.Invoke(new Error(
                    errorType: ErrorType.OrderInfoWrongAccessToken, 
                    errorMessage: "Access token is null or empty"
                ));

                return;
            }

            var url = PayStationUrlBuilder.GetPaystationHost(settings) + $"paystation2/api/payments/status?access_token={accessToken}";

            XDebug.Log(settings, $"[OrderInfo.Query] url={url}");

            WebRequestHelper.Instance.GetRequest<OrderInfo.Response>(sdkType, url, requestHeader: null,
                onComplete: response =>
                {
                    var invoices = response.invoices_data;
                    var invoice = FindInvoiceData(invoices);
                    
                    if (invoice == null) {
                        onFailure.Invoke(new Error(
                            errorType: ErrorType.OrderInfoNoInvoices, 
                            errorMessage: "No invoices found in the response"
                        ));

                        return;
                    }

                    if (!Enum.IsDefined(typeof(OrderInfo.Response.Status), invoice.status)) {
                        onFailure.Invoke(new Error(
                            errorType: ErrorType.OrderInfoInvalidStatus, 
                            errorMessage: "Invalid invoice status value: " + invoice.status
                        ));

                        return;
                    }

                    var status = (OrderInfo.Response.Status)invoice.status;

                    OrderInfo orderInfo;

                    if (status == OrderInfo.Response.Status.Done) {
                        if (invoice.invoice_id < 0) {
                            onFailure.Invoke(new Error(
                                errorType: ErrorType.OrderInfoDoneButInvalidInvoiceId, 
                                errorMessage: "Invalid invoice ID: " + invoice.invoice_id
                            ));

                            return;
                        }

                        if (invoice.order_id < 0) {
                            onFailure.Invoke(
                                new Error(
                                    errorType: ErrorType.OrderInfoDoneButInvalidOrderId, 
                                    errorMessage: "Invalid order ID: " + invoice.order_id,
                                    data: new Dictionary<string, string>
                                    {
                                        { "invoice_id", invoice.invoice_id.ToString() }
                                    }
                                )
                            );

                            return;
                        }

                        orderInfo = new OrderInfo.Done(invoice.order_id, invoice.invoice_id);
                    } else if (status == OrderInfo.Response.Status.Processing) {
                        orderInfo = new OrderInfo.Pending();
                    } else {
                        orderInfo = new OrderInfo.Canceled();
                    }

                    onSuccess.Invoke(orderInfo);
                },
                onError: error =>
                {
                    TokenAutoRefresher.Check(settings, error,
                        onError: error =>
                        {
                            onFailure.Invoke(new Error(
                                errorType: ErrorType.OrderInfoUnreachable, 
                                errorMessage: "Failed to reach order info service: " + error
                            ));
                        },
                        onSuccess: () => GetOrderInfo(settings, accessToken, sdkType, onSuccess, onFailure)
                    );
                }
            );
            
            OrderInfo.Response.InvoiceData FindInvoiceData(OrderInfo.Response.InvoiceData[] invoices)
            {
                if (invoices == null || invoices.Length == 0) {
                    return null;
                }
            
                for (int i = invoices.Length - 1; i >= 0; i--)
                {
                    var invoice = invoices[i];

                    var status = (OrderInfo.Response.Status)invoice.status;
                    if (status == OrderInfo.Response.Status.Done)
                        return invoice;
                }

                return invoices[invoices.Length - 1];
            }
        }
	}
}