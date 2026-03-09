using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xsolla.Core
{
    internal abstract class OrderInfo
    {
        public sealed class Pending : OrderInfo { }

        public sealed class Canceled : OrderInfo { }

        public sealed class Done : OrderInfo
        {
            public readonly long orderId;
            public readonly long invoiceId;

            public Done(long orderId, long invoiceId)
            {
                this.orderId = orderId;
                this.invoiceId = invoiceId;
            }
        }

        public bool TryAsDone(out Done done)
        {
            done = default;

            if (this is Done) {
                done = (Done)this;

                return true;
            }

            return false;
        }

        [Serializable]
        internal sealed class Response
        {
            [SerializeField] public InvoiceData[] invoices_data = Array.Empty<InvoiceData>();

            [Serializable]
            public sealed class InvoiceData
            {
                [SerializeField] public long invoice_id;
                [SerializeField] public int status;
                [SerializeField] public long order_id = -1;
            }

            public enum Status
            {
                [UsedImplicitly] Created = 1,
                Processing = 2,
                Done = 3,
                [UsedImplicitly] Canceled = 4,
                [UsedImplicitly] Error = 5,
                [UsedImplicitly] Authorized = 6,
                [UsedImplicitly] XsollaRefund = 7,
                [UsedImplicitly] XsollaRefundFailed = 8,
                [UsedImplicitly] Test = 9,
                [UsedImplicitly] Fraud = 10,
                [UsedImplicitly] CheckLenya = 11,
                [UsedImplicitly] Held = 12,
                [UsedImplicitly] Denied = 13,
                [UsedImplicitly] Stop = 14,
                [UsedImplicitly] Lost = 15,
                [UsedImplicitly] PartiallyRefunded = 16
            }
        }

        
    }
}
