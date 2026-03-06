using Assets.Scripts.Info;
using Assets.Scripts.ModelsReform;
using Newtonsoft.Json;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static Assets.Scripts.Info.BoardSpecInfo;

internal class DbManager : MonoBehaviour
{
    public static DbManager instance;
    public static string db_url;// = "http://192.168.0.28:2000/";

    static DbManager() {
        //string ip =  NetworkInterface.GetAllNetworkInterfaces()
        //    .Where(n => n.OperationalStatus == OperationalStatus.Up &&
        //                n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        //    .SelectMany(n => n.GetIPProperties().UnicastAddresses)
        //    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
        //    .Select(a => a.Address.ToString())
        //    .LastOrDefault();

        //db_url = $"http://{ip}:2000/";

    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        db_url = PlayerPrefs.GetString("DB_URL", "127.0.0.1:2000/");
        //GetObss(obss => {
        //    foreach (var obs in obss)
        //    {
        //        Debug.Log($"Obs: {obs.nameText} at ({obs.coordinate.x}, {obs.coordinate.y})");
        //    }
        //});

        //GetConf(conf => {
        //    Debug.Log($"Conf: proj={conf.projectName}, title={conf.titleName}");
        //});

        //GetBoardSpecs(brds =>
        //{
        //    foreach (var brd in brds)
        //    {
        //        Debug.Log($"board: {brd.modelCode} >>> ({brd.manufacturer}, {brd.descriptionText}) ({brd.sensorsDefaultMap.Count}, {brd.sensorsDefinitionMap.Count})");
        //    }
        //});

        //GetGroups(grps =>
        //{
        //    foreach (var grp in grps)
        //    {
        //        Debug.Log($"board: {grp.groupIdx} >>> ({grp.groupName})");
        //    }
        //});

        //GetPredTableRange(
        //    1, DateTime.Now.AddDays(-2), DateTime.Now,
        //    data =>
        //{
        //    foreach (var snapshot in data.table)
        //    {
        //        foreach (var valset in snapshot.valSet)
        //        {
        //            Debug.Log($" {snapshot.timestamp} time: {snapshot.timestamp} >> sensor {valset.idx} = {valset.val}");
        //        }
        //        //Debug.Log($"board: {brd.modelCode} >>> ({brd.manufacturer}, {brd.descriptionText}) ({brd.sensorsDefaultMap.Count}, {brd.sensorsDefinitionMap.Count})");
        //    }
        //})
        //GetAlarms(alms =>
        //{
        //    foreach (var alm in alms)
        //    {
        //        Debug.Log($"ala:{alm.alarmIdx}/obs:{alm.obsIdx}/ty:{alm.alarmType} >>> ({alm.occured.timestamp} ~ {(alm.solved.HasValue ? alm.solved.Value.timestamp : "진행중")})");
        //    }
        //});
    }

    #region [외부 인터페이스] 
    public async void GetObss(Action<List<ObservatoryInfo>> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetObssAsync();
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void GetAlarms(Action<List<AlarmInfo>> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetAlarmsAsync();
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void GetBoardSpecs(Action<List<BoardSpecInfo>> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetBoardSpecsAsync();
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void GetGroups(Action<List<GroupInfo>> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetGroupsAsync();
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void GetConf(Action<ConfigurationInfo> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetConfAsync();
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void GetMeasTableRange(int obsIdx, DateTime fromDt, DateTime toDt, Action<TimeSeriesInfo> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetMeasTableRangeAsync(obsIdx, fromDt, toDt);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void GetPredTableRange(int obsIdx, DateTime fromDt, DateTime toDt, Action<TimeSeriesInfo> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetPredTableRangeAsync(obsIdx, fromDt, toDt);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void SetRecentMeas(int obsIdx, DateTime datetime, Dictionary<int, float?> vals, Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await SetRecentMeasAsync(obsIdx, datetime, vals);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void RecordMeasData(int obsIdx, DateTime datetime, Dictionary<int, float?> vals, Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            Debug.LogError($"[db] RecordMeasData\n");
            var result = await RecordMeasDataAsync(obsIdx, datetime, vals);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void SetObsMeasPv(int obsIdx, Dictionary<int, float?> vals, Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await SetObsMeasPvAsync(obsIdx, vals);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void GetRecentMeas(int obsIdx, Action<SensorRowInfo> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetRecentMeasAsync(obsIdx);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }

    }
    public async void GetAlarmsActivated(Action<List<AlarmInfo>> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetAlarmsActivatedAsync();
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }

    }
    public async void ExecAlarmOccure(AlarmInfo occuredAlarm, Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await ExecAlarmOccureAsync(occuredAlarm);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void ExecAlarmSolve(AlarmInfo solvedAlarm, Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await ExecAlarmSolvedAsync(solvedAlarm);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void SetObsAlarmType(int obsIdx, Dictionary<int, AlarmState> sensorStates, Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await SetObsAlarmTypeAsync(obsIdx, sensorStates);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void SetObsBoardState(int obsIdx, BoardSpecInfo.BoardType boardType,
        string stateLife,
        string? stateOp,
        string? stateCom, Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await SetObsBoardStateAsync(obsIdx, boardType, stateLife, stateOp, stateCom);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }
    public async void SetRecentBoardState(int obsIdx, BoardSpecInfo.BoardType boardType,
        DateTime timestamp,
        string modelCode,
        string stateLife,
        string? stateOp,
        string? stateCom,
        Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await SetRecentBoardStateAsync(obsIdx, boardType, timestamp, modelCode, stateLife, stateOp, stateCom);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }

    }
    public async void GetRecentBoardState(int obsIdx,
        Action<BoardStateInfo> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetRecentBoardStateAsync(obsIdx);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }

    }
    public async void GetRecentBoardStateAll(int obsIdx,
        Action<List<BoardStateInfo>> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await GetRecentBoardStateAllAsync();
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }

    }
    public async void SetObsBoardOnOff(
        int obsIdx, BoardSpecInfo.BoardType boardType, bool isUsing, bool isInspecting,
        Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await SetObsBoardOnOffAsync(obsIdx, boardType, isUsing, isInspecting);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }

    public async void RequestResetThreshold(int obsIdx, Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await RequestResetThresholdAsync(obsIdx);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }


    public async void RequestResetInspect(int obsIdx, Action<bool> callback, Action<Exception> onError = null)
    {
        try
        {
            var result = await RequestResetInspectAsync(obsIdx);
            callback?.Invoke(result);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
            Debug.LogError(e.ToString());
        }
    }

    #endregion

    #region [내부 인터페이스]
    public async Task<List<ObservatoryInfo>> GetObssAsync(CancellationToken ct = default)
    {
        var query = "Select * from TB_OBS;";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);


        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);


        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<ObservatoryData>>(response), ct);


        return entity.Select(model => new ObservatoryInfo(model)).ToList();
    }
    private async Task<List<AlarmInfo>> GetAlarmsAsync(CancellationToken ct = default)
    {
        var query = "Select * from TB_ALARM;";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);
        //Debug.Log(response);

        // 서버가 "Error: ..." 문자열로 내려주는 형태를 그대로 쓰고 있어서,
        // 여기서 에러 문자열이면 예외로 바꿔주는게 유지보수에 유리함
        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        // JSON 파싱이 크면 메인 스레드에서 잠깐 튈 수 있음 → Task.Run으로 분리 가능
        // (Unity API 호출 없음 = 안전)
        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<AlarmData>>(response), ct);
        //Debug.Log("entity.Count : " + entity.Count.ToString());

        return entity.Select(model => new AlarmInfo(model)).ToList();
    }
    private async Task<List<BoardSpecInfo>> GetBoardSpecsAsync(CancellationToken ct = default)
    {
        var query = "Select * from TB_BOARD;";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        // 서버가 "Error: ..." 문자열로 내려주는 형태를 그대로 쓰고 있어서,
        // 여기서 에러 문자열이면 예외로 바꿔주는게 유지보수에 유리함
        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        // JSON 파싱이 크면 메인 스레드에서 잠깐 튈 수 있음 → Task.Run으로 분리 가능
        // (Unity API 호출 없음 = 안전)
        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<BoardSpecData>>(response), ct);
        //Debug.Log(response);
        //Debug.Log("entity.Count : " + entity.Count.ToString());

        return entity.Select(model => new BoardSpecInfo(model)).ToList();
    }
    private async Task<List<GroupInfo>> GetGroupsAsync(CancellationToken ct = default)
    {
        var query = "Select * from TB_GROUP;";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        // 서버가 "Error: ..." 문자열로 내려주는 형태를 그대로 쓰고 있어서,
        // 여기서 에러 문자열이면 예외로 바꿔주는게 유지보수에 유리함
        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        // JSON 파싱이 크면 메인 스레드에서 잠깐 튈 수 있음 → Task.Run으로 분리 가능
        // (Unity API 호출 없음 = 안전)
        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<GroupData>>(response), ct);
        //Debug.Log(response);
        //Debug.Log("entity.Count : " + entity.Count.ToString());

        return entity.Select(model => new GroupInfo(model)).ToList();
    }
    private async Task<ConfigurationInfo> GetConfAsync(CancellationToken ct = default)
    {
        var query = "Select * from TB_CONF;";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        // 서버가 "Error: ..." 문자열로 내려주는 형태를 그대로 쓰고 있어서,
        // 여기서 에러 문자열이면 예외로 바꿔주는게 유지보수에 유리함
        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        // JSON 파싱이 크면 메인 스레드에서 잠깐 튈 수 있음 → Task.Run으로 분리 가능
        // (Unity API 호출 없음 = 안전)
        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<ConfigurationData>>(response), ct);
        //Debug.Log(response);
        //Debug.Log("entity.Count : " + entity.Count.ToString());

        return entity.Select(model => new ConfigurationInfo(model)).ToList().First();
    }
    private async Task<TimeSeriesInfo> GetMeasTableRangeAsync(int obsIdx, DateTime fromDt, DateTime toDt, CancellationToken ct = default)
    {
        var query = $"Select * from TB_HNS_DATA_MEAS Where {fromDt: yyyyMMddHHmmss} <= MEAS_DT and MEAS_DT <= {toDt: yyyyMMddHHmmss} and OBS_IDX = {obsIdx};";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        // 서버가 "Error: ..." 문자열로 내려주는 형태를 그대로 쓰고 있어서,
        // 여기서 에러 문자열이면 예외로 바꿔주는게 유지보수에 유리함
        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        // JSON 파싱이 크면 메인 스레드에서 잠깐 튈 수 있음 → Task.Run으로 분리 가능
        // (Unity API 호출 없음 = 안전)
        Debug.Log("query : " + query);
        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<HnsDataMeasData>>(response), ct);
        //Debug.Log(response);
        //Debug.Log("entity.Count : " + entity.Count.ToString());

        return new TimeSeriesInfo(entity);
    }
    private async Task<TimeSeriesInfo> GetPredTableRangeAsync(int obsIdx, DateTime fromDt, DateTime toDt, CancellationToken ct = default)
    {
        var query = $"Select * from TB_HNS_DATA_PRED Where {fromDt: yyyyMMddHHmmss} <= PRED_DT and PRED_DT <= {toDt: yyyyMMddHHmmss} and OBS_IDX = {obsIdx};";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        // 서버가 "Error: ..." 문자열로 내려주는 형태를 그대로 쓰고 있어서,
        // 여기서 에러 문자열이면 예외로 바꿔주는게 유지보수에 유리함
        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        // JSON 파싱이 크면 메인 스레드에서 잠깐 튈 수 있음 → Task.Run으로 분리 가능
        // (Unity API 호출 없음 = 안전)
        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<HnsDataPredData>>(response), ct);
        //Debug.Log(response);
        //Debug.Log("entity.Count : " + entity.Count.ToString());

        return new TimeSeriesInfo(entity);
    }
    private async Task<bool> SetRecentMeasAsync(int obsIdx, DateTime datetime, Dictionary<int, float?> vals, CancellationToken ct = default)
    {
        var measDt = datetime.ToString("yyyyMMddHHmmss");

        var sensorArgs = vals
            .Where(kvp => kvp.Key >= 1 && kvp.Key <= 59)
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => $", @sensor_{kvp.Key:D3} = {(kvp.Value.HasValue ? kvp.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL")}");


        var query = $"EXEC EXEC_UPSERT_RECENT_MEAS" +
            $"    @obsIdx = {obsIdx}," +
            $"    @measDt = '{measDt}'" +
            string.Concat(sensorArgs) +
            ";";

        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;
    }
    private async Task<bool> RecordMeasDataAsync(int obsIdx, DateTime datetime, Dictionary<int, float?> vals, CancellationToken ct = default)
    {
        var measDt = datetime.ToString("yyyyMMddHHmm") + "00";

        var sensorArgs = vals
            .Where(kvp => kvp.Key >= 1 && kvp.Key <= 59)
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => $", @sensor_{kvp.Key:D3} = {(kvp.Value.HasValue ? kvp.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL")}");


        var query = $"EXEC EXEC_RECORD_DATA_MEAS" +
            $"    @OBS_IDX = {obsIdx}," +
            $"    @MEAS_DT = '{measDt}'" +
            string.Concat(sensorArgs) +
            ";";

        //Debug.LogError($"[db] REFRSH: {query}\n");
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;
    }
    private async Task<bool> SetObsMeasPvAsync(int obsIdx, Dictionary<int, float?> vals, CancellationToken ct = default)
    {

        var sensorArgs = vals
            .Where(kvp => kvp.Key >= 1 && kvp.Key <= 59)
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => $", @sensor_{kvp.Key:D3}_pv = {(kvp.Value.HasValue ? kvp.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL")}");


        var query = $"EXEC EXEC_OBS_UPDATE_PV" +
            $"    @obsIdx = {obsIdx}" +
            string.Concat(sensorArgs) +
            ";";

        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;
    }
    private async Task<SensorRowInfo> GetRecentMeasAsync(int obsIdx, CancellationToken ct = default)
    {
        var query = $"SELECT * FROM TB_HNS_RECENT_MEAS WHERE OBS_IDX = {obsIdx}";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        // 서버가 "Error: ..." 문자열로 내려주는 형태를 그대로 쓰고 있어서,
        // 여기서 에러 문자열이면 예외로 바꿔주는게 유지보수에 유리함
        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        if (response == "[]")
            throw new Exception("No data returned for recent measurement.");

        // JSON 파싱이 크면 메인 스레드에서 잠깐 튈 수 있음 → Task.Run으로 분리 가능
        // (Unity API 호출 없음 = 안전)
        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<HnsDataMeasData>>(response)?.First(), ct);

        if (entity == null)
            throw new Exception("No data returned for recent measurement.");

        return new SensorRowInfo(entity);

    }
    private async Task<List<AlarmInfo>> GetAlarmsActivatedAsync(CancellationToken ct = default)
    {
        var query = "Select * from TB_ALARM where SOLVED_DT is null;";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);
        //Debug.Log(response);

        // 서버가 "Error: ..." 문자열로 내려주는 형태를 그대로 쓰고 있어서,
        // 여기서 에러 문자열이면 예외로 바꿔주는게 유지보수에 유리함
        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        // JSON 파싱이 크면 메인 스레드에서 잠깐 튈 수 있음 → Task.Run으로 분리 가능
        // (Unity API 호출 없음 = 안전)
        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<AlarmData>>(response), ct);
        //Debug.Log("entity.Count : " + entity.Count.ToString());

        return entity.Select(model => new AlarmInfo(model)).ToList();
    }
    private async Task<bool> ExecAlarmOccureAsync(AlarmInfo occuredAlarm, CancellationToken ct = default)
    {

        var query = $"EXEC EXEC_ALARM_OCCURE" +
            $"  @obsIdx = {occuredAlarm.obsIdx}," +
            $"  @alarmType = {occuredAlarm.alarmType}," +
            $"  @obsName = N'{occuredAlarm.obsNameText}'," +
            $"  @obsAddress = N'{occuredAlarm.obsAddrText}'," +
            $"" +
            $"  @boardModelCode = '{occuredAlarm.boardModelCode}'," +
            $"  @sensorIdx = {(occuredAlarm.sensorIdx.HasValue ? occuredAlarm.sensorIdx.Value : "null")}," +
            $"" +
            $"  @occuredValueMeas = {occuredAlarm.occured.valMeas}," +
            $"  @occuredValuePred = null," +
            $"  @occuredThresholdHigh = {occuredAlarm.occured.thresholdHigh}," +
            $"  @occuredThresholdLow = {occuredAlarm.occured.thresholdLow}," +
            $"  @occuredTimestamp = '{occuredAlarm.occured.timestamp}'," +
            $"  @sensorName = '{occuredAlarm.sensorName}';";

        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;

    }
    private async Task<bool> ExecAlarmSolvedAsync(AlarmInfo solvedAlarm, CancellationToken ct = default)
    {
        var solvedInfo = solvedAlarm.solved.Value;

        var query = $"EXEC EXEC_ALARM_SOLVE" +
            $"  @alarmIdx = {solvedAlarm.alarmIdx}," +
            $"  @solvedValueMeas = {solvedInfo.valMeas}," +
            $"  @solvedValuePred = null," +
            $"  @solvedThresholdHigh = {solvedInfo.thresholdHigh}," +
            $"  @solvedThresholdLow = {solvedInfo.thresholdLow}," +
            $"  @solvedTimestamp = '{solvedInfo.timestamp}';";
        //Debug.LogError("[DEBUG] : " + query);
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;
    }
    private async Task<bool> SetObsAlarmTypeAsync(int obsIdx, Dictionary<int, AlarmState> sensorStates, CancellationToken ct = default)
    {

        var sensorArgs = sensorStates
            .Where(kvp => kvp.Key >= 1 && kvp.Key <= 59)
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => $", @sensor_{kvp.Key:D3}_alarm_type = {(int)kvp.Value}");


        var query = $"EXEC EXEC_OBS_UPDATE_ALARM_TYPE" +
            $"    @obsIdx = {obsIdx}" +
            string.Concat(sensorArgs) +
            ";";

        //Debug.LogError("[DEBUG] : " + query);
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;

    }
    private async Task<bool> SetObsBoardStateAsync(int obsIdx, BoardSpecInfo.BoardType boardType,
        string stateLife,
        string? stateOp = null,
        string? stateCom = null,
        CancellationToken ct = default)
    {
        var query = $"EXEC EXEC_UPDATE_BOARD_STATE" +
            $"    @obsIdx = {obsIdx}," +
            $"    @boardKind = '{boardType.ToString()}'," +

            $"    @lifeState  = {(stateLife == null ? "NULL" : "'" + stateLife.ToString() + "'")}," +
            $"    @opState  = {(stateOp == null ? "NULL" : "'" + stateOp.ToString() + "'")}," +
            $"    @comState  = {(stateCom == null ? "NULL" : "'" + stateCom.ToString() + "'")}" +
            ";";

        //Debug.LogError("[DEBUG] : " + query);
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;

    }
    private async Task<bool> SetRecentBoardStateAsync(int obsIdx, BoardSpecInfo.BoardType boardType,
        DateTime timestamp,
        string modelCode = null,
        string stateLife = null,
        string? stateOp = null,
        string? stateCom = null,
        CancellationToken ct = default)
    {
        var query = 
            $"EXEC EXEC_UPSERT_RECENT_BOARD_STATE" +
            $"@obsIdx = {obsIdx}," +
            $"@boardKind = '{boardType.ToString()}'," +
            $"@modelCode = '{modelCode}'," +
            $"@timestamp = '{timestamp.ToString("yyyyMMddHHmmss")}'," +
            $"@lifeState = '{stateLife}'," +
            $"@opState  = {(stateOp == null ? "NULL" : "'" + stateOp.ToString() + "'")}," +
            $"@comState  = {(stateCom == null ? "NULL" : "'" + stateCom.ToString() + "'")}" +
            ";";

        //Debug.LogError("[DEBUG] : " + query);
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;

    }
    private async Task<BoardStateInfo> GetRecentBoardStateAsync(int obsIdx, CancellationToken ct = default)
    {
        var query = $"SELECT * FROM TB_BOARD_RECENT_STATE WHERE OBS_IDX = {obsIdx};";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);


        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);
        if (response == "[]")
            throw new Exception("No data returned for recent board state.");

        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<BoardStateData>>(response)?.First(), ct);

        if (entity == null)
            throw new Exception("No data returned for recent board state.");
        return new BoardStateInfo(entity);
    }
    private async Task<bool> SetObsBoardOnOffAsync(int obsIdx, BoardSpecInfo.BoardType boardType, bool isUsing, bool isInspecting, CancellationToken ct = default)
    {
        var query = $"EXEC EXEC_UPDATE_BOARD_STATE" +
            $"    @obsIdx = {obsIdx}," +
            $"    @boardKind = '{boardType.ToString()}'," +

            $"    @useYn  = {(isUsing ? "'Y'" : "'N'")}," +
            $"    @inspectYn   = {(isInspecting ? "'Y'" : "'N'")}" +
            ";";

        Debug.LogError("[DEBUG] : " + query);
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;

    }
    private async Task<List<BoardStateInfo>> GetRecentBoardStateAllAsync(CancellationToken ct = default)
    {
        var query = $"SELECT * FROM TB_BOARD_RECENT_STATE;";
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);


        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);
        if (response == "[]")
            throw new Exception("No data returned for recent board state.");

        var entity = await Task.Run(() =>
            JsonConvert.DeserializeObject<List<BoardStateData>>(response), ct);

        if (entity == null)
            throw new Exception("No data returned for recent board state.");
        return entity.Select(ent => new BoardStateInfo(ent)).ToList();
    }
    private async Task<bool> RequestResetThresholdAsync(int obsIdx, CancellationToken ct = default)
    {
        var query = $"EXEC dbo.EXEC_RESET_SENSOR_DEFINITION " +
                    $"@obsIdx = {obsIdx};";

        Debug.LogError("[DEBUG] : " + query);
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;

    }
    private async Task<bool> RequestResetInspectAsync(int obsIdx, CancellationToken ct = default)
    {
        var query = $"EXEC dbo.EXEC_RESET_SENSOR_USE_YN " +
                    $"@obsIdx = {obsIdx};";

        Debug.LogError("[DEBUG] : " + query);
        string response = await ResponseAPIStringAsync(QueryType.SELECT.ToString(), query, ct);

        if (response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            throw new Exception(response);

        return true;
    }

    #endregion


    /// <summary>
    /// DB서버를 연결해주는 API서버에 쿼리문을 전달한 뒤, 응답을 전달받는 함수입니다. (코루틴 없이)
    /// </summary>
    public async Task<string> ResponseAPIStringAsync(string type, string query, CancellationToken ct = default)
    {
        var data = new
        {
            SQLType = type,
            SQLquery = query
        };

        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        byte[] jsonToSend = Encoding.UTF8.GetBytes(json);

        using var request = new UnityWebRequest(db_url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 요청 시작
        var op = request.SendWebRequest();

        // CancellationToken 지원(요청 취소)
        using var reg = ct.Register(() =>
        {
            try { request.Abort(); } catch { /* ignore */ }
        });

        // 코루틴 없이 완료 대기
        await op.AsTask();

        if (request.result == UnityWebRequest.Result.Success)
        {
            return request.downloadHandler.text;
        }

        // request.error는 정보가 빈약할 때가 있어서 result도 같이 포함
        return $"Error: {request.result} / {request.error}";
    }


    enum QueryType
    {
        SELECT,
        UPDATE
    }
}

/// <summary>
/// Unity AsyncOperation을 await 가능하도록 Task로 감싸는 확장
/// </summary>
public static class UnityAsyncExtensions
{
    public static Task AsTask(this AsyncOperation op)
    {
        if (op.isDone) return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>();
        op.completed += _ => tcs.TrySetResult(true);
        return tcs.Task;
    }
}
