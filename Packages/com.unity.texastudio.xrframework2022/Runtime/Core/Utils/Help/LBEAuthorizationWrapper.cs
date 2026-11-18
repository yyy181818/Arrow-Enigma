using AOT;
using System;
using System.Runtime.InteropServices;
using TexaXRFramework.Core.Net;
using Unity.XR.PICO.TOBSupport;
using UnityEngine;
namespace TexaXRFramework.Core
{
    public static class LBEAuthorizationWrapper
    {
        const long VERIFY_RESULT_OK = 0;
        // 未找到授权记录
        const long BizErrors_BIZ_ERROR_NO_AUTH_RECORDS = 10030;
        // 无效密钥
        const long BizErrors_BIZ_ERROR_INVALID_TOKEN = 10031;
        // 密钥已过期
        const long BizErrors_BIZ_ERROR_TOKEN_EXPIRED = 10032;
        // 设备未授权
        const long BizErrors_BIZ_ERROR_NO_AUTH_DEVICE = 10033;
        // 超过授权有效期
        const long BizErrors_BIZ_ERROR_EXCEED_AUTH_EXPIRE_DATE = 10034;
        // 超过授权订单数上限
        const long BizErrors_BIZ_ERROR_EXCEED_AUTH_ORDER_LIMIT = 10035;
        // 提交的参数信息错误
        const long VERIFY_RESULT_PARAM_ERROR = 50001;
        // 本地校验设备信息不匹配
        const long VERIFY_RESULT_HARDWARE = 50002;
        // 服务器错误，可能是链接超时，或者返回这不是200
        const long VERIFY_RESULT_SERVER_ERROR = 50003;
        // 服务器返回结果解析错误
        const long VERIFY_RESULT_PARSE_ERROR = 50004;
        // 上一次调用没有返回就调用了下一次
        const long VERIFY_RESULT_REPEAT = 50005;
        // 服务器校验失败
        const long VERIFY_RESULT_FAILED = 60000;

        public delegate void VerifyResultCallback(long value, int status);

        [DllImport("lbelic", CallingConvention = CallingConvention.Cdecl)]
        private static extern void VerifyLic(IntPtr json, System.IntPtr callback);

        [DllImport("lbelic", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetPersistentDataPath(IntPtr path);

        private static VerifyResultCallback _callback;

        [MonoPInvokeCallback(typeof(VerifyResultCallback))]
        public static void OnResultCallback(long value, int status)
        {
            //Debug.Log("Call OnResultCallback");
            _callback?.Invoke(value, status);
        }
        public static void Verify(string json, VerifyResultCallback callback)
        {
            //Debug.Log("Call Verify");
            IntPtr pathPtr = Marshal.StringToHGlobalAnsi(Application.persistentDataPath);
            SetPersistentDataPath(pathPtr);
            _callback = callback;
            IntPtr strPtr = Marshal.StringToHGlobalAnsi(json);
            VerifyResultCallback cb = OnResultCallback;

            System.IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(cb);
            VerifyLic(strPtr, callbackPtr);
            Marshal.FreeHGlobal(strPtr);
        }

        public class VerifyInfo
        {
            public long venue_id;
            public long application_id;
            // 多长视角要网络请求校验， 单位是小时
            public long verify_time;
            public string device;
            public string token;
            public string url;

            // 正式环境为空，
            public string env;
            public bool use_device_pc_debug;
            public bool use_net;
            public bool debug_log;
            public bool remove_localfile;
        }

        public static void CheckAuthorizationStatus(string venue_id, string application_id, string token, string verify_time, string sn, HeadlessStartOptions headless)
        {
            VerifyInfo info = new VerifyInfo();
            info.verify_time = int.Parse(verify_time) * 24;// 24;
            info.venue_id = long.Parse(venue_id);
            info.application_id = long.Parse(application_id);
            info.device = sn;
            info.token = token;
            if (headless == HeadlessStartOptions.OfflineStartClient)
            {
                info.env = "ppe_pico_lbe_token";
                info.env = "";
                //info.url = "http???";
                info.use_device_pc_debug = true;

                info.use_net = true;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                PXR_Enterprise.InitEnterpriseService();
                PXR_Enterprise.BindEnterpriseService((flag) =>
                {
                    string sn = PXR_Enterprise.StateGetDeviceInfo(SystemInfoEnum.EQUIPMENT_SN);
                    info.device = sn;
                    string json = JsonUtility.ToJson(info);
                    Debug.Log("[鉴权] 开始申请鉴权:json: " + json);
                    Verify(json, (val, status) =>
                    {
                        VerifyResult(val, status);
                    });
                });
            }
            else 
            {
                string json = JsonUtility.ToJson(info);
                Debug.Log("[鉴权] 开始申请鉴权:json: " + json);
                Verify(json, (val, status) =>
                {
                    VerifyResult(val, status);
                });
            }       
        }

        public static bool Authorization_Detection_Completed = false;
        public static bool AuthStatus = true;
        public static string AuthMsg;

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="status">1 本地  2 网络</param>
        public static void VerifyResult(long value, int status)
        {
            Debug.Log($"[鉴权] 完成申请鉴权返回数据 ==> value {value} status {status}");
            AuthMsg = value.ToString();
            string result = "鉴权成功";
            switch (value)
            {
                case VERIFY_RESULT_FAILED:
                    AuthStatus = false;
                    result = "鉴权失败";
                    break;
                case VERIFY_RESULT_PARAM_ERROR:
                    AuthStatus = false;
                    result = "提交的参数信息错误";
                    break;
                case VERIFY_RESULT_HARDWARE:
                    AuthStatus = false;
                    result = "本地校验设备信息不匹配";
                    break;
                case VERIFY_RESULT_SERVER_ERROR:
                    AuthStatus = false;
                    result = "服务器错误，可能是链接超时，或者返回这不是200";
                    break;
                case VERIFY_RESULT_PARSE_ERROR:
                    AuthStatus = false;
                    result = "服务器返回结果解析错误";
                    break;
                case VERIFY_RESULT_REPEAT:
                    AuthStatus = false;
                    result = "上一次调用没有返回就调用了下一次";
                    break;
                case BizErrors_BIZ_ERROR_NO_AUTH_RECORDS:
                    AuthStatus = false;
                    result = "未找到授权记录";
                    break;
                case BizErrors_BIZ_ERROR_INVALID_TOKEN:
                    AuthStatus = false;
                    result = "无效密钥";
                    break;
                case BizErrors_BIZ_ERROR_TOKEN_EXPIRED:
                    AuthStatus = false;
                    result = "密钥已过期";
                    break;
                case BizErrors_BIZ_ERROR_NO_AUTH_DEVICE:
                    AuthStatus = false;
                    result = "设备未授权";
                    break;
                case BizErrors_BIZ_ERROR_EXCEED_AUTH_EXPIRE_DATE:
                    AuthStatus = false;
                    result = "超过授权有效期";
                    break;
                case BizErrors_BIZ_ERROR_EXCEED_AUTH_ORDER_LIMIT:
                    AuthStatus = false;
                    result = "超过授权订单数上限";
                    break;
            }
            Debug.Log($"[鉴权] 鉴权结果: {result}");
            Authorization_Detection_Completed = true;
            LBENetworkSyncManager.Instance.NetworkLogic.SyncAuthResultToServer();
        }
    }
}
