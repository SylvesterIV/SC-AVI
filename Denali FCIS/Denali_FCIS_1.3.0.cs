public IMyTextPanel Debug;
public IMyTerminalBlock progBlock;


String redirectTag;
String allowTag;
String displayName;
String clearScreen;

bool allowRedirect = true;
public List<IMyRadioAntenna> antennaBlocks = new List<IMyRadioAntenna>();
IMyBroadcastListener bcListner;
String lastArgument = "0";
int count = 0;

bool RangeBracket = false;

string RangeId = "0";

Vector3D currentPos;
Vector3D vectorToTarget;
double distanceToTarget;
Vector3D prevPosition;
Vector3D velocity;
public Vector3D targetPos = Vector3D.Zero;
public Vector3D targetVelocity = Vector3D.Zero;

public static WcPbApi api;
public List<MyDefinitionId> weaponDefinitions = new List<MyDefinitionId>();
public List<string> definitionSubIds = new List<string>();


public IMyTextPanel debugLCD; // Debug LCD panel
public string broadcastTag = "none"; // Tag for broadcasting messages
public string channelTag = "none"; // Tag for receiving messages

IMyBroadcastListener _myBroadcastListener;

public string sourceLCDName = "MCR Data LCD"; // Name of the source LCD
public string auxLCDName = "MCR AUX LCD"; // Name of the source LCD
public string targetLCDName = "MCR Receive LCD"; // Name of the target LCD for receiving broadcasts
public string statusLCDName = "MCR Status LCD";

public string HUD_ID_TX = " ";
public string HUD_ID_RX = " ";

public string auxtext = " ";


public string Callsign = "Unassigned";

public string WarningStatus = " ";

public string ChosenCall = " ";



public static Dictionary<string, string> RangeDict = new Dictionary<string, string>() { 

      {"0", ""}, 
      {".5", "collision"},
      {"1", "range_1km"}, 
      {"2", "range_2km"},
      {"3", "range_3km"},
      {"4", "range_4km"}, 
      {"5", "range_5km"}, 
      {"6", "range_6km"}, 
      {"7", "range_7km"}
                   
};



public Program()
{
  Runtime.UpdateFrequency = UpdateFrequency.Update10;
  
  
  

   // Initialize debug LCD panel for the broadcaster
  debugLCD = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("Debug1");
  
  Debug = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("Debug");
  //GridTerminalSystem.SearchBlocksOfName("Prog", progBlock);
  GridTerminalSystem.GetBlocksOfType(antennaBlocks);  
  progBlock = Me;
   
  //foreach (IMyRadioAntenna antenna in antennaBlocks) antenna.EnableBroadcasting = false;    
  
  
  var cData = progBlock.CustomData.Split(new[] {"\n"}, StringSplitOptions.None);
  redirectTag = cData[0];
  allowTag = cData[1];
  displayName = cData[2];
  clearScreen = cData[3];
  
  
  Debug = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(displayName);
  bcListner = IGC.RegisterBroadcastListener(redirectTag);
  
  
  _myBroadcastListener = IGC.RegisterBroadcastListener(channelTag);
  
  api = new WcPbApi();
  try {
      api.Activate(Me);
  }
  catch {
      Echo("WeaponCore Api is failing! \n Make sure WeaponCore is enabled!"); 
		   return;
  }  
  
}


public void Main(string argument)
{
    
    //Clear Debug Screen if clear set true
    if (clearScreen == "True") Debug.WriteText(Callsign);
    
    
    //Listen for redirect target command and execute if redirects allowed
    MyIGCMessage message;
    while (bcListner.HasPendingMessage) 
    {
      message = bcListner.AcceptMessage();
      if(message.Tag == redirectTag && allowRedirect)
      {
        long target = (long)message.Data;
        api.SetAiFocus(progBlock, target);
      }
    }


    IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(auxLCDName) as IMyTextPanel;
      if (lcd != null)
        {
        auxtext = lcd.GetText();
        
        }


    //get current target info and display it
    MyDetectedEntityInfo? info = api.GetAiFocus(Me.CubeGrid.EntityId);
    StatusUpdate(info);
    if (info.HasValue && !info.Value.IsEmpty()) 
    {
     
      Debug.WriteText(String.Format("\nTarget Name: {0}\n", info.Value.Name), true);
      Debug.WriteText(String.Format("Distance to Target: {0}\n", (int)distanceToTarget), true);
      Debug.WriteText(String.Format("Relative Target Speed: {0}",  -info.Value.Velocity.Length()), true);
      //Debug.WriteText(String.Format("Relative Target Velocity: {0} {1} {2}\n", (int)targetVelocity.X, (int)targetVelocity.Y, (int)targetVelocity.Z), true);
     
      Debug.WriteText(string.Format(auxtext), true);
      Debug.WriteText(string.Format("\n"), true);
      Debug.WriteText(string.Format(WarningStatus), true);
    
      
    }
    

    
    
    
    //check run arguments and handle them over multiple updates. Primarly to allow antennas to have broadcast off most of the time.
    if (argument != "") lastArgument = argument;    
    if (lastArgument == redirectTag)
    {        
      //foreach (IMyRadioAntenna antenna in antennaBlocks) antenna.EnableBroadcasting = true; 
      count++;
      if (count > 4) 
      {
        IGC.SendBroadcastMessage(redirectTag, info.Value.EntityId);      
        lastArgument = "";
        count = 0;
        //foreach (IMyRadioAntenna antenna in antennaBlocks) antenna.EnableBroadcasting = false; 
      }      
    }
    else if (lastArgument == allowTag)
    {
      if (allowRedirect == true) allowRedirect = false;
      else allowRedirect = true;
      lastArgument = "";
    }
    
    
    //display current state of redirect system
    Debug.WriteText(String.Format(" ", allowRedirect), true);

    // Read text from source LCD and broadcast it
    string textToBroadcast = ReadTextFromLCD(sourceLCDName);
    
    BroadcastTextToLCD(textToBroadcast, targetLCDName);

   
    // Receive and display broadcasted text
    ReceiveAndDisplayBroadcastedText(targetLCDName);

     

     IMyTerminalBlock SAP = GridTerminalSystem.GetBlockWithName("Select Apollo") as IMyTimerBlock;
     if(SAP != null){
        if((SAP as IMyFunctionalBlock).Enabled==true) Channel1(); 
        SAP.ApplyAction("OnOff_Off");
     }
     
      IMyTerminalBlock SAR = GridTerminalSystem.GetBlockWithName("Select Artemis") as IMyTimerBlock;
     if(SAR != null){
        if((SAR as IMyFunctionalBlock).Enabled==true) Channel2(); 
        SAR.ApplyAction("OnOff_Off");
     }

      IMyTerminalBlock SAT = GridTerminalSystem.GetBlockWithName("Select Taranis") as IMyTimerBlock;
     if(SAT != null){
        if((SAT as IMyFunctionalBlock).Enabled==true) Channel3(); 
        SAT.ApplyAction("OnOff_Off");
     }

      IMyTerminalBlock STH = GridTerminalSystem.GetBlockWithName("Select Thor") as IMyTimerBlock;
     if(STH != null){
        if((STH as IMyFunctionalBlock).Enabled==true) Channel4(); 
        STH.ApplyAction("OnOff_Off");
     }

      IMyTerminalBlock SHA = GridTerminalSystem.GetBlockWithName("Select Hadad") as IMyTimerBlock;
     if(SHA != null){
        if((SHA as IMyFunctionalBlock).Enabled==true) Channel5(); 
        SHA.ApplyAction("OnOff_Off");
     }

          

     
     
     
     
    //  Channel1(); // Set the Reciever tag depending on the argument received
    //  if ("RX2" == argument) Channel2();
    //  if ("RX3" == argument) Channel3(); // Set the Reciever tag depending on the argument received
    //  if ("RX4" == argument) Channel4();
    //  if ("RX5" == argument) Channel5();
    
     if ("TX1" == argument) Broadcast1(); // Set the broadcast tag depending on the argument received
     if ("TX2" == argument) Broadcast2();
     if ("TX3" == argument) Broadcast3(); // Set the broadcast tag depending on the argument received
     if ("TX4" == argument) Broadcast4();
     if ("TX5" == argument) Broadcast5(); // Set the broadcast tag depending on the argument received

     
     
   

    IMyTextPanel lcdS = GridTerminalSystem.GetBlockWithName(statusLCDName) as IMyTextPanel; // display what channel you're broadcasting to on your status LCD
      
    lcdS.WriteText(HUD_ID_RX);

    if (api.HasGridAi(Me.CubeGrid.EntityId))
    {
        MyTuple<bool, int, int> data = api.GetProjectilesLockedOn(Me.CubeGrid.EntityId);

        if (data.Item2 > 0)
        {
            WarningStatus = "MISSILE WARNING";
        }
        else
        {
             WarningStatus = " ";
        }
        Echo(data.Item2.ToString());
    }

   
    
    if(distanceToTarget <= 7100 && distanceToTarget >= 6900)
    {
        RangeId = "7";
        CallRange();
        RangeBracket = true;
        
    } 
     else if(distanceToTarget <= 6100 && distanceToTarget >= 5900)
    {    
        RangeId = "6";
        CallRange();
        RangeBracket = true;
        
    }
     else if(distanceToTarget <= 5100 && distanceToTarget >= 4900)
    {
        RangeId = "5";
        CallRange();
        RangeBracket = true;
        
    }
     else if(distanceToTarget <= 4100 && distanceToTarget >= 3900)
    {
        RangeId = "4";
        CallRange();
        RangeBracket = true;
        
    }
     else if(distanceToTarget <= 3100 && distanceToTarget >= 2900)
    {
        RangeId = "3";
        CallRange();
        RangeBracket = true;
        
    }
    else if(distanceToTarget <= 2100 && distanceToTarget >= 1900)
    {
         RangeId = "2";
        CallRange();
        RangeBracket = true;
       
    }
    else if(distanceToTarget <= 1100 && distanceToTarget >= 900)
    {
        RangeId = "1";
        CallRange();
        RangeBracket = true;
        
    }
    else if(distanceToTarget <= 110 && distanceToTarget >= 0)
    {
        RangeId = ".5";
        CallRange();
        RangeBracket = true;
        
    }
    else 
    {
        RangeId = "0";
        
        CallRange();

        RangeBracket = false;
    }
    
}
public void CallRange()
{
    if(RangeBracket == false)
    {

         string ChosenCall = RangeDict[RangeId];

        IMySoundBlock MySoundBlock;
        MySoundBlock = GridTerminalSystem.GetBlockWithName("FCIS Speaker") as IMySoundBlock; 
        if(MySoundBlock != null)
        {
            
            MySoundBlock.SelectedSound = ChosenCall;
            MySoundBlock.Play();

        }


    }




}

public void Channel1()
{
    channelTag = "Channel1";
    HUD_ID_RX = "RX C1"; // for displaying what channel you are receiving from
    _myBroadcastListener = IGC.RegisterBroadcastListener(channelTag);
}

public void Channel2()
{
    channelTag = "Channel2";
    HUD_ID_RX = "RX C2"; // for displaying what channel you are receiving from
    _myBroadcastListener = IGC.RegisterBroadcastListener(channelTag);
}
public void Channel3()
{
    channelTag = "Channel3";
    HUD_ID_RX = "RX C3"; // for displaying what channel you are receiving from
    _myBroadcastListener = IGC.RegisterBroadcastListener(channelTag);
}

public void Channel4()
{
    channelTag = "Channel4";
    HUD_ID_RX = "RX C4"; // for displaying what channel you are receiving from
    _myBroadcastListener = IGC.RegisterBroadcastListener(channelTag);
}
public void Channel5()
{
    channelTag = "Channel5";
    HUD_ID_RX = "RX C5"; // for displaying what channel you are receiving from
    _myBroadcastListener = IGC.RegisterBroadcastListener(channelTag);
}





public void Broadcast1()
{
    broadcastTag = "Channel1";
    HUD_ID_TX = "TX C1"; // for displaying what channel you are broadcasting to
    Callsign = "Callsign: Apollo";
    IMySoundBlock MySoundBlock;
        MySoundBlock = GridTerminalSystem.GetBlockWithName("FCIS Speaker") as IMySoundBlock; 
        if(MySoundBlock != null)
        {
            
            MySoundBlock.SelectedSound = "comms_online";
            MySoundBlock.Play();

        }
}

public void Broadcast2()
{
    broadcastTag = "Channel2";
    HUD_ID_TX = "TX C2";
    Callsign = "Callsign: Artemis";
     IMySoundBlock MySoundBlock;
        MySoundBlock = GridTerminalSystem.GetBlockWithName("FCIS Speaker") as IMySoundBlock; 
        if(MySoundBlock != null)
        {
            
            MySoundBlock.SelectedSound = "comms_online";
            MySoundBlock.Play();

        }
}
public void Broadcast3()
{
    broadcastTag = "Channel3";
    HUD_ID_TX = "TX C3"; // for displaying what channel you are broadcasting to
    Callsign = "Callsign: Taranis";
     IMySoundBlock MySoundBlock;
        MySoundBlock = GridTerminalSystem.GetBlockWithName("FCIS Speaker") as IMySoundBlock; 
        if(MySoundBlock != null)
        {
            
            MySoundBlock.SelectedSound = "comms_online";
            MySoundBlock.Play();

        }
    
}

public void Broadcast4()
{
    broadcastTag = "Channel4";
    HUD_ID_TX = "TX C4";
    Callsign = "Callsign: Thor";
     IMySoundBlock MySoundBlock;
        MySoundBlock = GridTerminalSystem.GetBlockWithName("FCIS Speaker") as IMySoundBlock; 
        if(MySoundBlock != null)
        {
            
            MySoundBlock.SelectedSound = "comms_online";
            MySoundBlock.Play();

        }
}
public void Broadcast5()
{
    broadcastTag = "Channel5";
    HUD_ID_TX = "TX C5"; // for displaying what channel you are broadcasting to
    Callsign = "Callsign: Hadad";
     IMySoundBlock MySoundBlock;
        MySoundBlock = GridTerminalSystem.GetBlockWithName("FCIS Speaker") as IMySoundBlock; 
        if(MySoundBlock != null)
        {
            
            MySoundBlock.SelectedSound = "comms_online";
            MySoundBlock.Play();

        }
    
}








string ReadTextFromLCD(string sourceLCDName)
{
    IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(sourceLCDName) as IMyTextPanel;
    if (lcd != null)
    {
        string text = lcd.GetText();
        return text;
    }
    else
    {
    
        return "";
    }
}

void BroadcastTextToLCD(string text, string sourceLCDName)
{
   
    IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(sourceLCDName) as IMyTextPanel;
    if (lcd != null && !string.IsNullOrEmpty(text))
    {
        IGC.SendBroadcastMessage(broadcastTag, text);
    }
    else if (lcd == null)
    {
        debugLCD.WriteText($"Error: LCD '{sourceLCDName}' not found!");
    }
    else
    {
        debugLCD.WriteText($"Error: Text to broadcast is empty!");
    }
}

void ReceiveAndDisplayBroadcastedText(string targetLCDName)
{
    

    while (_myBroadcastListener.HasPendingMessage)
    {
        var msg = _myBroadcastListener.AcceptMessage();
        if (msg.Tag == channelTag)
        {
            string text = msg.Data.ToString();
            DisplayTextOnLCD(text, targetLCDName);
        }
    }
}

void DisplayTextOnLCD(string text, string targetLCDName)
{
    IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(targetLCDName) as IMyTextPanel;
    if (lcd != null)
    {
        lcd.WriteText(text);
    }
    else
    {
        debugLCD.WriteText($"Error: LCD '{targetLCDName}' not found!");
    }
}





//update target info such as range, velocity, ect.
void StatusUpdate(MyDetectedEntityInfo? info)
{
    currentPos = progBlock.GetPosition();
    if (info.HasValue && !info.Value.IsEmpty()) {
        targetPos = info.Value.Position;
        velocity = (prevPosition - currentPos)* 7;
        targetVelocity = -info.Value.Velocity - velocity; //Relative target velocity
    }
    vectorToTarget = currentPos - targetPos;
    distanceToTarget = vectorToTarget.Length();
    vectorToTarget.Normalize();
    prevPosition = currentPos;



   
}   


public class WcPbApi
{
    private Action<ICollection<MyDefinitionId>> _getCoreWeapons;
    private Action<ICollection<MyDefinitionId>> _getCoreStaticLaunchers;
    private Action<ICollection<MyDefinitionId>> _getCoreTurrets;
    private Func<IMyTerminalBlock, IDictionary<string, int>, bool> _getBlockWeaponMap;
    private Func<long, MyTuple<bool, int, int>> _getProjectilesLockedOn;
    private Action<IMyTerminalBlock, IDictionary<MyDetectedEntityInfo, float>> _getSortedThreats;
    private Func<long, int, MyDetectedEntityInfo> _getAiFocus;
    private Func<IMyTerminalBlock, long, int, bool> _setAiFocus;
    private Func<IMyTerminalBlock, int, MyDetectedEntityInfo> _getWeaponTarget;
    private Action<IMyTerminalBlock, long, int> _setWeaponTarget;
    private Action<IMyTerminalBlock, bool, int> _fireWeaponOnce;
    private Action<IMyTerminalBlock, bool, bool, int> _toggleWeaponFire;
    private Func<IMyTerminalBlock, int, bool, bool, bool> _isWeaponReadyToFire;
    private Func<IMyTerminalBlock, int, float> _getMaxWeaponRange;
    private Func<IMyTerminalBlock, ICollection<string>, int, bool> _getTurretTargetTypes;
    private Action<IMyTerminalBlock, ICollection<string>, int> _setTurretTargetTypes;
    private Action<IMyTerminalBlock, float> _setBlockTrackingRange;
    private Func<IMyTerminalBlock, long, int, bool> _isTargetAligned;
    private Func<IMyTerminalBlock, long, int, bool> _canShootTarget;
    private Func<IMyTerminalBlock, long, int, Vector3D?> _getPredictedTargetPos;
    private Func<IMyTerminalBlock, float> _getHeatLevel;
    private Func<IMyTerminalBlock, float> _currentPowerConsumption;
    private Func<MyDefinitionId, float> _getMaxPower;
    private Func<long, bool> _hasGridAi;
    private Func<IMyTerminalBlock, bool> _hasCoreWeapon;
    private Func<long, float> _getOptimalDps;
    private Func<IMyTerminalBlock, int, string> _getActiveAmmo;
    private Action<IMyTerminalBlock, int, string> _setActiveAmmo;
    private Action<Action<Vector3, float>> _registerProjectileAdded;
    private Action<Action<Vector3, float>> _unRegisterProjectileAdded;
    private Func<long, float> _getConstructEffectiveDps;
    private Func<IMyTerminalBlock, long> _getPlayerController;
    private Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, int, Matrix> _getWeaponAzimuthMatrix;
    private Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, int, Matrix> _getWeaponElevationMatrix;

    public bool Activate(IMyTerminalBlock pbBlock)
    {
        var dict = pbBlock.GetProperty("WcPbAPI")?.As<IReadOnlyDictionary<string, Delegate>>().GetValue(pbBlock);
        if (dict == null) throw new Exception($"WcPbAPI failed to activate");
        return ApiAssign(dict);
    }

    public bool ApiAssign(IReadOnlyDictionary<string, Delegate> delegates)
    {
        if (delegates == null)
            return false;
        AssignMethod(delegates, "GetCoreWeapons", ref _getCoreWeapons);
        AssignMethod(delegates, "GetCoreStaticLaunchers", ref _getCoreStaticLaunchers);
        AssignMethod(delegates, "GetCoreTurrets", ref _getCoreTurrets);
        AssignMethod(delegates, "GetBlockWeaponMap", ref _getBlockWeaponMap);
        AssignMethod(delegates, "GetProjectilesLockedOn", ref _getProjectilesLockedOn);
        AssignMethod(delegates, "GetSortedThreats", ref _getSortedThreats);
        AssignMethod(delegates, "GetAiFocus", ref _getAiFocus);
        AssignMethod(delegates, "SetAiFocus", ref _setAiFocus);
        AssignMethod(delegates, "GetWeaponTarget", ref _getWeaponTarget);
        AssignMethod(delegates, "SetWeaponTarget", ref _setWeaponTarget);
        AssignMethod(delegates, "FireWeaponOnce", ref _fireWeaponOnce);
        AssignMethod(delegates, "ToggleWeaponFire", ref _toggleWeaponFire);
        AssignMethod(delegates, "IsWeaponReadyToFire", ref _isWeaponReadyToFire);
        AssignMethod(delegates, "GetMaxWeaponRange", ref _getMaxWeaponRange);
        AssignMethod(delegates, "GetTurretTargetTypes", ref _getTurretTargetTypes);
        AssignMethod(delegates, "SetTurretTargetTypes", ref _setTurretTargetTypes);
        AssignMethod(delegates, "SetBlockTrackingRange", ref _setBlockTrackingRange);
        AssignMethod(delegates, "IsTargetAligned", ref _isTargetAligned);
        AssignMethod(delegates, "CanShootTarget", ref _canShootTarget);
        AssignMethod(delegates, "GetPredictedTargetPosition", ref _getPredictedTargetPos);
        AssignMethod(delegates, "GetHeatLevel", ref _getHeatLevel);
        AssignMethod(delegates, "GetCurrentPower", ref _currentPowerConsumption);
        AssignMethod(delegates, "GetMaxPower", ref _getMaxPower);
        AssignMethod(delegates, "HasGridAi", ref _hasGridAi);
        AssignMethod(delegates, "HasCoreWeapon", ref _hasCoreWeapon);
        AssignMethod(delegates, "GetOptimalDps", ref _getOptimalDps);
        AssignMethod(delegates, "GetActiveAmmo", ref _getActiveAmmo);
        AssignMethod(delegates, "SetActiveAmmo", ref _setActiveAmmo);
        AssignMethod(delegates, "RegisterProjectileAdded", ref _registerProjectileAdded);
        AssignMethod(delegates, "UnRegisterProjectileAdded", ref _unRegisterProjectileAdded);
        AssignMethod(delegates, "GetConstructEffectiveDps", ref _getConstructEffectiveDps);
        AssignMethod(delegates, "GetPlayerController", ref _getPlayerController);
        AssignMethod(delegates, "GetWeaponAzimuthMatrix", ref _getWeaponAzimuthMatrix);
        AssignMethod(delegates, "GetWeaponElevationMatrix", ref _getWeaponElevationMatrix);
        return true;
    }

    private void AssignMethod<T>(IReadOnlyDictionary<string, Delegate> delegates, string name, ref T field) where T : class
    {
        if (delegates == null) {
            field = null;
            return;
        }
        Delegate del;
        if (!delegates.TryGetValue(name, out del))
            throw new Exception($"{GetType().Name} :: Couldn't find {name} delegate of type {typeof(T)}");
        field = del as T;
        if (field == null)
            throw new Exception(
                $"{GetType().Name} :: Delegate {name} is not type {typeof(T)}, instead it's: {del.GetType()}");
    }
    public void GetAllCoreWeapons(ICollection<MyDefinitionId> collection) => _getCoreWeapons?.Invoke(collection);
    public void GetAllCoreStaticLaunchers(ICollection<MyDefinitionId> collection) =>
        _getCoreStaticLaunchers?.Invoke(collection);
    public void GetAllCoreTurrets(ICollection<MyDefinitionId> collection) => _getCoreTurrets?.Invoke(collection);
    public bool GetBlockWeaponMap(IMyTerminalBlock weaponBlock, IDictionary<string, int> collection) =>
        _getBlockWeaponMap?.Invoke(weaponBlock, collection) ?? false;
    public MyTuple<bool, int, int> GetProjectilesLockedOn(long victim) =>
        _getProjectilesLockedOn?.Invoke(victim) ?? new MyTuple<bool, int, int>();
    public void GetSortedThreats(IMyTerminalBlock pbBlock, IDictionary<MyDetectedEntityInfo, float> collection) =>
        _getSortedThreats?.Invoke(pbBlock, collection);
    public MyDetectedEntityInfo? GetAiFocus(long shooter, int priority = 0) => _getAiFocus?.Invoke(shooter, priority);
    public bool SetAiFocus(IMyTerminalBlock pbBlock, long target, int priority = 0) =>
        _setAiFocus?.Invoke(pbBlock, target, priority) ?? false;
    public MyDetectedEntityInfo? GetWeaponTarget(IMyTerminalBlock weapon, int weaponId = 0) =>
        _getWeaponTarget?.Invoke(weapon, weaponId) ?? null;
    public void SetWeaponTarget(IMyTerminalBlock weapon, long target, int weaponId = 0) =>
        _setWeaponTarget?.Invoke(weapon, target, weaponId);
    public void FireWeaponOnce(IMyTerminalBlock weapon, bool allWeapons = true, int weaponId = 0) =>
        _fireWeaponOnce?.Invoke(weapon, allWeapons, weaponId);
    public void ToggleWeaponFire(IMyTerminalBlock weapon, bool on, bool allWeapons, int weaponId = 0) =>
        _toggleWeaponFire?.Invoke(weapon, on, allWeapons, weaponId);
    public bool IsWeaponReadyToFire(IMyTerminalBlock weapon, int weaponId = 0, bool anyWeaponReady = true,
        bool shootReady = false) =>
        _isWeaponReadyToFire?.Invoke(weapon, weaponId, anyWeaponReady, shootReady) ?? false;
    public float GetMaxWeaponRange(IMyTerminalBlock weapon, int weaponId) =>
        _getMaxWeaponRange?.Invoke(weapon, weaponId) ?? 0f;
    public bool GetTurretTargetTypes(IMyTerminalBlock weapon, IList<string> collection, int weaponId = 0) =>
        _getTurretTargetTypes?.Invoke(weapon, collection, weaponId) ?? false;
    public void SetTurretTargetTypes(IMyTerminalBlock weapon, IList<string> collection, int weaponId = 0) =>
        _setTurretTargetTypes?.Invoke(weapon, collection, weaponId);
    public void SetBlockTrackingRange(IMyTerminalBlock weapon, float range) =>
        _setBlockTrackingRange?.Invoke(weapon, range);
    public bool IsTargetAligned(IMyTerminalBlock weapon, long targetEnt, int weaponId) =>
        _isTargetAligned?.Invoke(weapon, targetEnt, weaponId) ?? false;
    public bool CanShootTarget(IMyTerminalBlock weapon, long targetEnt, int weaponId) =>
        _canShootTarget?.Invoke(weapon, targetEnt, weaponId) ?? false;
    public Vector3D? GetPredictedTargetPosition(IMyTerminalBlock weapon, long targetEnt, int weaponId) =>
        _getPredictedTargetPos?.Invoke(weapon, targetEnt, weaponId) ?? null;
    public float GetHeatLevel(IMyTerminalBlock weapon) => _getHeatLevel?.Invoke(weapon) ?? 0f;
    public float GetCurrentPower(IMyTerminalBlock weapon) => _currentPowerConsumption?.Invoke(weapon) ?? 0f;
    public float GetMaxPower(MyDefinitionId weaponDef) => _getMaxPower?.Invoke(weaponDef) ?? 0f;
    public bool HasGridAi(long entity) => _hasGridAi?.Invoke(entity) ?? false;
    public bool HasCoreWeapon(IMyTerminalBlock weapon) => _hasCoreWeapon?.Invoke(weapon) ?? false;
    public float GetOptimalDps(long entity) => _getOptimalDps?.Invoke(entity) ?? 0f;
    public string GetActiveAmmo(IMyTerminalBlock weapon, int weaponId) =>
        _getActiveAmmo?.Invoke(weapon, weaponId) ?? null;
    public void SetActiveAmmo(IMyTerminalBlock weapon, int weaponId, string ammoType) =>
        _setActiveAmmo?.Invoke(weapon, weaponId, ammoType);
    public void RegisterProjectileAddedCallback(Action<Vector3, float> action) =>
        _registerProjectileAdded?.Invoke(action);
    public void UnRegisterProjectileAddedCallback(Action<Vector3, float> action) =>
        _unRegisterProjectileAdded?.Invoke(action);
    public float GetConstructEffectiveDps(long entity) => _getConstructEffectiveDps?.Invoke(entity) ?? 0f;
    public long GetPlayerController(IMyTerminalBlock weapon) => _getPlayerController?.Invoke(weapon) ?? -1;
    public Matrix GetWeaponAzimuthMatrix(Sandbox.ModAPI.Ingame.IMyTerminalBlock weapon, int weaponId) =>
        _getWeaponAzimuthMatrix?.Invoke(weapon, weaponId) ?? Matrix.Zero;
    public Matrix GetWeaponElevationMatrix(Sandbox.ModAPI.Ingame.IMyTerminalBlock weapon, int weaponId) =>
        _getWeaponElevationMatrix?.Invoke(weapon, weaponId) ?? Matrix.Zero;
}
