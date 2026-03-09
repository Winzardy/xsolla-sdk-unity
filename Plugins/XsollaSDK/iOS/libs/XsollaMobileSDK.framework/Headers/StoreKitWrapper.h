//
//  StoreKitWrapper.h
//  XsollaMobileSDK
//
//  Created by Vsevolod Klementjev on 12/02/2024.
//

#ifndef StoreKitWrapper_h
#define StoreKitWrapper_h

#import <XsollaMobileSDK/XsollaMobileSDK-Swift.h>

typedef SKXMutablePayment SKMutablePayment;
typedef SKXPayment SKPayment;
typedef SKXPaymentQueue SKPaymentQueue;
typedef SKXPaymentTransaction SKPaymentTransaction;
typedef SKXProduct SKProduct;
typedef SKXProductDiscount SKProductDiscount;
typedef SKXProductsRequest SKProductsRequest;
typedef SKXProductsResponse SKProductsResponse;
typedef SKXRequest SKRequest;

#define SKRequestDelegate SKXRequestDelegate
#define SKProductsRequestDelegate SKXProductsRequestDelegate
#define SKPaymentTransactionObserver SKXPaymentTransactionObserver

#endif /* StoreKitWrapper_h */
