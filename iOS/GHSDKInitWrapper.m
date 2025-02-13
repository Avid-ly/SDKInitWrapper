//
//  GHSDKInitWrapper.m
//  MeteorShowSDK
//
//  Created by steve on 2024/12/20.
//  Copyright © 2024 . All rights reserved.
//

#import "GHSDKInitWrapper.h"
#import "AppsFlyerLib/AppsFlyerLib.h"
#import <FirebaseAnalytics/FirebaseAnalytics.h>
#import <FirebaseCore/FirebaseCore.h>
#import <TraceAnalysisSDK/TraceAnalysis.h>
#if __has_include(<MSSDK/MSSDK.h>)
#import <MSSDK/MSSDK.h>
#else
#import "MSSDK.h"
#endif


@implementation GHSDKInitWrapper

BOOL GHSDKIsInitFirebase;   // Firebase不能初始化多次（初始化多次会崩溃），故记录下初始化状态防止多次初始化

+ (void)initSDK:(void(^)(void))handler {
    // AF设置
    // 初始化参数
    [[AppsFlyerLib shared] setAppsFlyerDevKey:@"your devKey"];
    [[AppsFlyerLib shared] setAppleAppID:@"your appId"];
    // 开启AF自动获取UMP结果模式
    [[AppsFlyerLib shared] enableTCFDataCollection:YES];
    
    // MSSDK UMP
    UIViewController *rootVC = [UIApplication sharedApplication].keyWindow.rootViewController;
    [MSSDK umpRequestConsentInfoUpdateAndShow:rootVC completion:^(NSError *error) {
        if (error) {
            // 授权失败
            
            // 调用AF的start方法（当UMP失败后）
            [[AppsFlyerLib shared] start];
        }
        else {
            // 授权成功
            
            // 调用AF的start方法（当UMP完成后）
            [[AppsFlyerLib shared] start];
            
            // 初始化MSSDK
            [MSSDK initSDKCompletion:^{
                // MSSDK 初始化完成
            }];
            
            if (![MSSDK umpCanRequestAds]) {
                NSLog(@"UMP cannotRequestAds");
                
                // 传递UMP授权结果给Firebase（未授权）
                [FIRAnalytics setConsent:@{
                    FIRConsentTypeAnalyticsStorage : FIRConsentStatusDenied,
                    FIRConsentTypeAdStorage : FIRConsentStatusDenied,
                    FIRConsentTypeAdUserData : FIRConsentStatusDenied,
                    FIRConsentTypeAdPersonalization : FIRConsentStatusDenied,
                }];
                // Firebase Analytics 初始化
                if (!GHSDKIsInitFirebase) {
                    [FIRApp configure];
                    GHSDKIsInitFirebase = YES;
                }
            }
            else {
                NSLog(@"UMP canRequestAds");
                
                // 传递UMP授权结果给Firebase（已授权）
                [FIRAnalytics setConsent:@{
                    FIRConsentTypeAnalyticsStorage : FIRConsentStatusGranted,
                    FIRConsentTypeAdStorage : FIRConsentStatusGranted,
                    FIRConsentTypeAdUserData : FIRConsentStatusGranted,
                    FIRConsentTypeAdPersonalization : FIRConsentStatusGranted,
                }];
                // Firebase Analytics 初始化
                if (!GHSDKIsInitFirebase) {
                    [FIRApp configure];
                    GHSDKIsInitFirebase = YES;
                }
            }
        }
        
        if (handler) {
            handler();
        }
    }];
}

@end
