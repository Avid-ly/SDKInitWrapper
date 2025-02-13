package com.example.GHSdkInitWrapper;

import android.app.Activity;
import android.content.Context;
import android.util.Log;
import android.view.View;
import android.widget.Toast;

import androidx.annotation.NonNull;

import com.aly.sdk.ALYAnalysis;
import com.appsflyer.AppsFlyerLib;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.firebase.analytics.FirebaseAnalytics;
import com.ms.sdk.MsInterstitialAd;
import com.ms.sdk.MsRewardVideoAd;
import com.ms.sdk.MsSDK;
import com.ms.sdk.listener.MsSdkInitializationListener;
import com.ms.sdk.ump.GoogleUmpUtil;
import com.ms.sdk.wrapper.interstitial.MsInterstitialAdListener;
import com.ms.sdk.wrapper.interstitial.MsInterstitialLoadCallback;
import com.ms.sdk.wrapper.video.MsRewardVideoAdListener;
import com.ms.sdk.wrapper.video.MsRewardVideoLoadCallback;


import java.util.HashMap;

/***
 * 调用示例：
 *
 * wrapper = new GHSdkInitWrapper(this,this);
 * wrapper.initSdk();
 */
public class GHSdkInitWrapper {
    private Context context;
    private Activity activity;

    private MsRewardVideoAd mVideoAd;
    private MsInterstitialAd mInterstitialAd;


    private static  final String TAG = "sdkLog";
    //todo 修改tasdk的pid cid
    private String productId = "your productId";
    private String channelId = "your channelId";

    //todo 修改appsflyer的sdk key
    private String afKey = "your afKey";

    //todo 修改debug模式，上线前修改成false
    private Boolean debugFlag = true;

    public SdkWrapper(Context context, Activity activity){
        this.context = context;
        this.activity = activity;
    }

    public void initSdk(){
        //appsflyer
        AppsFlyerLib.getInstance().setDebugLog(debugFlag);
        AppsFlyerLib.getInstance().init(afKey, null, context);
        AppsFlyerLib.getInstance().enableTCFDataCollection(true);

        ALYAnalysis.enalbeDebugMode(debugFlag);
        ALYAnalysis.init(context, productId, channelId, new ALYAnalysis.TasdkinitializdListener() {
            @Override
            public void onSuccess(String userid) {
                ALYAnalysis.enalbeDebugMode(debugFlag);
                if (debugFlag){
                    Log.i(TAG, "init success userId is   " + userid);
                }
                //将id给appsflyer
                String openId=ALYAnalysis.getOpenId(context);
                AppsFlyerLib.getInstance().setCustomerUserId(openId);

                // 将afid赋值给统计包
                ALYAnalysis.setAFId(AppsFlyerLib.getInstance().getAppsFlyerUID(context));

                //firebase
                HashMap<FirebaseAnalytics.ConsentType, FirebaseAnalytics.ConsentStatus> consentMap = new HashMap<FirebaseAnalytics.ConsentType, FirebaseAnalytics.ConsentStatus>();
                consentMap.put(FirebaseAnalytics.ConsentType.ANALYTICS_STORAGE, FirebaseAnalytics.ConsentStatus.GRANTED);
                FirebaseAnalytics.getInstance(context).setConsent(consentMap);
                FirebaseAnalytics.getInstance(context).getAppInstanceId().addOnCompleteListener(new OnCompleteListener<String>() {
                    @Override
                    public void onComplete(@NonNull com.google.android.gms.tasks.Task<String> task) {
                        if (task.isSuccessful()) {
                            String firebaseId = task.getResult();
                            ALYAnalysis.setFirebaseId(firebaseId);
                        }
                    }
                });

                //ump
                GoogleUmpUtil googleUmpUtil = GoogleUmpUtil.getInstance(context);
                googleUmpUtil.gatherConsent(activity, error -> {
                if (error == null){
                    if (googleUmpUtil.canRequestAds()){
                        AppsFlyerLib.getInstance().start(context);
                        initMssdk();
                    }
                }else {
                    Log.d(TAG, "ump error: "+error);
                }
                });

            }

            @Override
            public void onFail(String errorMsg) {
                Log.i(TAG, "init error  " + errorMsg);
            }
        });

        //tasdk在线时间上报
        ALYAnalysis.openAlyAutoDuration(context);
    }


    private void initMssdk(){
        MsSDK.setDebuggable(debugFlag);
        MsSDK.init(context, new MsSdkInitializationListener() {
            @Override
            public void onInitializationSuccess() {
                if (debugFlag){
                    Log.i(TAG, "mssdk初始化成功: ");
                }
            }

            @Override
            public void onInitializationFail(String s) {
                Log.i(TAG, "mssdk初始化失败: " + s);
            }
        });
    }
}
