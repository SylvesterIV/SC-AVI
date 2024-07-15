
   
    





    // watch functional blocks
// v240607.2

const string BLOCKS = @"
L1G
L2G
R1G
R2G
";

int activecount = 0;

List<IMyTerminalBlock> tbs = new List<IMyTerminalBlock>();
List<string> controllers = new List<string> { "DSS L1", "DSS L2", "DSS R1", "DSS R2" };

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    foreach( string s in BLOCKS.Split('\n') ){
        if( "" == s )  continue;
        IMyTerminalBlock tb = GridTerminalSystem.GetBlockWithName( s.Trim() );
        if( null != tb )  tbs.Add(tb); else Echo( "block \"" + s + "\" not found. skip" );
    }
    //List<IMyTerminalBlock> ts = new List<IMyTerminalBlock>();  GridTerminalSystem.GetBlocks( ts );
    //foreach( IMyTerminalBlock t in ts )  Me.CustomData = Me.CustomData + t.CustomName + "\n";
}

public void Main(string argument, UpdateType updateSource) {
    string txt = "";
    
    
    IMyTextPanel lcd = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("MCR AUX LCD");
    if(lcd != null){
    lcd.WriteText("");
    }


    foreach( IMyTerminalBlock tb in tbs ){
        if( tb is IMyFunctionalBlock){
            IMyFunctionalBlock fb = tb as IMyFunctionalBlock;
            switch( fb.Enabled & fb.IsFunctional ){
                case true:  txt += ""; break;
                case false:  txt += ""; break;
            }
        } else {
            txt += "";
        }

        IMySlimBlock sb = tb.CubeGrid.GetCubeBlock(tb.Position);
        if( null != sb ){
            float pct = 100f * ( sb.BuildIntegrity - sb.CurrentDamage ) / sb.MaxIntegrity;
            txt += "  " + pct.ToString("0").PadLeft( 3, ' ' ) + "%";
        } else {
            txt += " ";
        }

        
    }  

  

            if(lcd != null){
            //lcd.WriteText("[L1]  [L2]  [R2]  [R1]\n", txt);
            lcd.WriteText("\n  [L1]     [L2]     [R2]    [R1]\n" + txt);
           
            }
            
    
    
    
    
    
    
    
    
    IMyTerminalBlock MSC = GridTerminalSystem.GetBlockWithName("DSS Mode Switch") as IMyTimerBlock;
    
    if(MSC != null){
        if((MSC as IMyFunctionalBlock).Enabled==true) ModeSwitch(); //DSS Stage toggle. Switches between stage 1 (Full forward fire) and stage 2 (alternating fire)
        MSC.ApplyAction("OnOff_Off");
    }   
    
    IMyTerminalBlock TSC = GridTerminalSystem.GetBlockWithName("DSS L/R Controller") as IMyTimerBlock;
    if(TSC != null){
        if((TSC as IMyFunctionalBlock).Enabled==true && activecount <= 2) CycleGuns(); 
        TSC.ApplyAction("OnOff_Off");
    }   
    
    

activecount = 0;
        
        
    foreach(string controller in controllers ){
            IMyFunctionalBlock cont = GridTerminalSystem.GetBlockWithName(controller) as IMyFunctionalBlock;
            if(cont != null){
                if(cont.Enabled == true){
                activecount = activecount + 1;

                }     
            }  
    } 
   

    
}
    public void CycleGuns(){
   
    //Weapon sequencing
   IMyTerminalBlock L1C = GridTerminalSystem.GetBlockWithName("DSS L1") as IMyTimerBlock;
    if(L1C != null){
    L1C.ApplyAction("OnOff");
    }

    IMyTerminalBlock L2C = GridTerminalSystem.GetBlockWithName("DSS L2") as IMyTimerBlock;
    if(L2C != null){
    L2C.ApplyAction("OnOff");
    }

    IMyTerminalBlock R2C = GridTerminalSystem.GetBlockWithName("DSS R2") as IMyTimerBlock;
    if(R2C != null){
    R2C.ApplyAction("OnOff");
    }

    IMyTerminalBlock R1C = GridTerminalSystem.GetBlockWithName("DSS R1") as IMyTimerBlock;
    if(R1C != null){
    R1C.ApplyAction("OnOff");
    }



    //Indicator lights
    IMyTerminalBlock R1CI = GridTerminalSystem.GetBlockWithName("R1Indicator") as IMyTerminalBlock;
    if(R1CI != null){
    R1CI.ApplyAction("OnOff");
    }

    IMyTerminalBlock L1CI = GridTerminalSystem.GetBlockWithName("L1Indicator") as IMyTerminalBlock;
    if(L1CI != null){
    L1CI.ApplyAction("OnOff");
    }

    IMyTerminalBlock L2CI = GridTerminalSystem.GetBlockWithName("L2Indicator") as IMyTerminalBlock;
    if(L2CI != null){
    L2CI.ApplyAction("OnOff");
    }

    IMyTerminalBlock R2CI = GridTerminalSystem.GetBlockWithName("R2Indicator") as IMyTerminalBlock;
    if(R2CI != null){
    R2CI.ApplyAction("OnOff");
    }

} 

public void ModeSwitch(){

   
        
    

    if(activecount <= 2){

        IMyTerminalBlock L1DEnable = GridTerminalSystem.GetBlockWithName("DSS L1") as IMyTimerBlock;
                if(L1DEnable != null){
                L1DEnable.ApplyAction("OnOff_On");
                }
        IMyTerminalBlock L2DEnable = GridTerminalSystem.GetBlockWithName("DSS L2") as IMyTimerBlock;
            if(L2DEnable != null){
            L2DEnable.ApplyAction("OnOff_On");
                }
        IMyTerminalBlock R1DEnable = GridTerminalSystem.GetBlockWithName("DSS R1") as IMyTimerBlock;
                if(R1DEnable != null){
                R1DEnable.ApplyAction("OnOff_On");
                }
        IMyTerminalBlock R2DEnable = GridTerminalSystem.GetBlockWithName("DSS R2") as IMyTimerBlock;
            if(R2DEnable != null){
            R2DEnable.ApplyAction("OnOff_On");
                }
        //Turn all Indicator lights on
                IMyTerminalBlock R1CI = GridTerminalSystem.GetBlockWithName("R1Indicator") as IMyTerminalBlock;
                if(R1CI != null){
                R1CI.ApplyAction("OnOff_On");
                }

                IMyTerminalBlock L1CI = GridTerminalSystem.GetBlockWithName("L1Indicator") as IMyTerminalBlock;
                if(L1CI != null){
                L1CI.ApplyAction("OnOff_On");
                }

                IMyTerminalBlock L2CI = GridTerminalSystem.GetBlockWithName("L2Indicator") as IMyTerminalBlock;
                if(L2CI != null){
                L2CI.ApplyAction("OnOff_On");
                }

                IMyTerminalBlock R2CI = GridTerminalSystem.GetBlockWithName("R2Indicator") as IMyTerminalBlock;
                if(R2CI != null){
                R2CI.ApplyAction("OnOff_On");
                }

    } else if (activecount >= 2){

        IMyTerminalBlock L1DEnable = GridTerminalSystem.GetBlockWithName("DSS L1") as IMyTimerBlock;
                if(L1DEnable != null){
                L1DEnable.ApplyAction("OnOff_Off");
                }
        IMyTerminalBlock L2DEnable = GridTerminalSystem.GetBlockWithName("DSS L2") as IMyTimerBlock;
            if(L2DEnable != null){
            L2DEnable.ApplyAction("OnOff_Off");
                }
        IMyTerminalBlock R1DEnable = GridTerminalSystem.GetBlockWithName("DSS R1") as IMyTimerBlock;
                if(R1DEnable != null){
                R1DEnable.ApplyAction("OnOff_On");
                }
        IMyTerminalBlock R2DEnable = GridTerminalSystem.GetBlockWithName("DSS R2") as IMyTimerBlock;
            if(R2DEnable != null){
            R2DEnable.ApplyAction("OnOff_On");
                }

                IMyTerminalBlock R1CI = GridTerminalSystem.GetBlockWithName("R1Indicator") as IMyTerminalBlock;
                if(R1CI != null){
                R1CI.ApplyAction("OnOff_On");
                }

                IMyTerminalBlock L1CI = GridTerminalSystem.GetBlockWithName("L1Indicator") as IMyTerminalBlock;
                if(L1CI != null){
                L1CI.ApplyAction("OnOff_Off");
                }

                IMyTerminalBlock L2CI = GridTerminalSystem.GetBlockWithName("L2Indicator") as IMyTerminalBlock;
                if(L2CI != null){
                L2CI.ApplyAction("OnOff_Off");
                }

                IMyTerminalBlock R2CI = GridTerminalSystem.GetBlockWithName("R2Indicator") as IMyTerminalBlock;
                if(R2CI != null){
                R2CI.ApplyAction("OnOff_On");
                }
    }
}



            
   
