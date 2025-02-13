using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Ump.Api;  // UMP
using UPTrace;  // TASDK
using AppsFlyerSDK; // AppsFlyerSDK
using Polymer;  // MSSDK
using Firebase; // FIrebase

namespace Gamehaus
{
    public class GHSDKWrapper
    {

        private static GHSDKWrapper sdkWrapper = null;

        private static void instanceOfCall()
        {
            if (sdkWrapper == null)
            {
                sdkWrapper = new GHSDKWrapper();
            }
        }

        public static void initSDK()
        {
            if (null == sdkWrapper)
            {
                instanceOfCall();
            }
            
            sdkWrapper.initFirebaseSDK();
            sdkWrapper.initAppsFlyerSDK();
            sdkWrapper.initUmpSDK();
        }

        // 初始化UMP SDK
        private void initUmpSDK()
        {
            // 测试ump的必要配置
            // var debugSettings = new ConsentDebugSettings
            // {   // 设置测试地区是EEA
            //     DebugGeography = DebugGeography.EEA,
            //     TestDeviceHashedIds = new List<string>
            //      {
            //          "6B268B4102A1961812FC511632EEAB9B" //请填写自己的测试设备ID
            //      }
            // };
            
            // 设置了reset每次都会弹出ump弹框，用于测试
            //ConsentInformation.Reset();

            // 设置参数
            ConsentRequestParameters request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false, // 年龄不受限制
                // ConsentDebugSettings = debugSettings, //追加debug设置
            };


            // 检查当前的用户请求状态
            Debug.Log("UMP ConsentInformation Request");
            ConsentInformation.Update(request, OnConsentInfoUpdated);
            // 初始化广告组件
        }

        // UMP 授权回调
        void OnConsentInfoUpdated(FormError consentError)
        {
            // 收到用户授权状态回调
            Debug.Log("UMP ConsentInformation Response");

            // 获取授权信息失败
            if (consentError != null)
            {
                Debug.Log("UMP ConsentInformation Response Error:" + consentError);
                UnityEngine.Debug.LogError(consentError);

                umpCompletion();
                return;
            }
            
            // 获取授权信息成功
            Debug.Log("UMP ConsentInformation Response Succeed");

            // 加载并展示UMP弹窗
            Debug.Log("UMP LoadAndShowConsentForm");
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
            {
                // 加载或展示弹窗失败
                if (formError != null)
                {
                    Debug.Log("UMP LoadAndShowConsentForm Error:" + formError);
                    UnityEngine.Debug.LogError(consentError);

                    umpCompletion();
                    return;
                }

                // 成功完成UMP授权
                Debug.Log("UMP LoadAndShowConsentForm Succeed");

                umpCompletion();
            });
        }

        // UMP 流程完成
        void umpCompletion() {

            // 初始化TASDK（不用管用户UMP授权结果具体是什么）
            initTASDK();
            // 调用AppsFlyer的start方法（不用管用户UMP授权结果具体是什么）
            startAppsFlyer();
            // 初始化MSSDK（广告聚合）
            initMSSDK();
            // 传递UMP授权结果给FIrebase
            setFirebaseConsent();
        }

        // 初始化TASDK
        void initTASDK() {
            string productId = "Your TASDK ProductId";
            string channelId = "Your TASDK ChannelId";
            UPTraceApi.initTraceSDKWithCallback(productId, channelId, initSuccessCallback, initFailCallback);
        }
        // TASDK 初始化成功回递
        void initSuccessCallback(string msg) {
            Debug.Log("TASDK initSuccessCallback ");
        }
        // TASDK初始化失败回调
        void initFailCallback(string message)
        {
            Debug.Log("TASDK initFailCallback ,message =" + message);
        }


        // 初始化AppsFlyer
        void initAppsFlyerSDK() {
            string devkey = "your AppsFlyer devkey";
            string appID = "your AppsFlyer appId";
            AppsFlyer.initSDK(devkey, appID);
            AppsFlyer.enableTCFDataCollection(true);
            
        }
        // 调用AppsFlyer的start方法
        void startAppsFlyer() {
            AppsFlyer.startSDK();
        }

        // 初始化MSSDK
        void initMSSDK() {
            MSSDK.initSdk(new System.Action<string>(mssdkInitCompleted),new System.Action<string>(mssdkInitFailed));
        }
        // MSSDK 初始化成功回调
        void mssdkInitCompleted(string str) {
            Debug.Log ("===> InitSDKCompleted Callback at: " + str);
        }
        // MSSDK 初始化失败回调
        void mssdkInitFailed(string str) {
            Debug.Log ("===> InitSDKFailed Callback at: " + str);
        }

        // 初始化FIrebase
        void initFirebaseSDK() {

        }
        // 传递UMP授权结果给FIrebase
        void setFirebaseConsent() {
            // UMP弹窗完成之后，获取授权结果（根据CanRequestAds）
            if (ConsentInformation.CanRequestAds()) {
                // 传递UMP授权结果给Firebase（已授权）
                Dictionary<Firebase.Analytics.ConsentType, Firebase.Analytics.ConsentStatus> consentSettings = new Dictionary<Firebase.Analytics.ConsentType, Firebase.Analytics.ConsentStatus>();
                consentSettings[Firebase.Analytics.ConsentType.AdStorage] = Firebase.Analytics.ConsentStatus.Granted;
                consentSettings[Firebase.Analytics.ConsentType.AnalyticsStorage] = Firebase.Analytics.ConsentStatus.Granted;
                consentSettings[Firebase.Analytics.ConsentType.AdUserData] = Firebase.Analytics.ConsentStatus.Granted;
                consentSettings[Firebase.Analytics.ConsentType.AdPersonalization] = Firebase.Analytics.ConsentStatus.Granted;
                Firebase.Analytics.FirebaseAnalytics.SetConsent(consentSettings);
            }
            else {
                // 传递UMP授权结果给Firebase（未授权）
                Dictionary<Firebase.Analytics.ConsentType, Firebase.Analytics.ConsentStatus> consentSettings = new Dictionary<Firebase.Analytics.ConsentType, Firebase.Analytics.ConsentStatus>();
                consentSettings[Firebase.Analytics.ConsentType.AdStorage] = Firebase.Analytics.ConsentStatus.Denied;
                consentSettings[Firebase.Analytics.ConsentType.AnalyticsStorage] = Firebase.Analytics.ConsentStatus.Denied;
                consentSettings[Firebase.Analytics.ConsentType.AdUserData] = Firebase.Analytics.ConsentStatus.Denied;
                consentSettings[Firebase.Analytics.ConsentType.AdPersonalization] = Firebase.Analytics.ConsentStatus.Denied;
                Firebase.Analytics.FirebaseAnalytics.SetConsent(consentSettings);
            }
        }
    }

}
