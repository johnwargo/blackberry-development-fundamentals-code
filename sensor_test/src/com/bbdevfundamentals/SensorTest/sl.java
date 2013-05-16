package com.bbdevfundamentals.SensorTest;

import net.rim.device.api.system.SensorListener;
import net.rim.device.api.ui.component.Dialog;

public class sl implements SensorListener {

	public void onSensorUpdate(int sensorId, int update){
		Dialog.alert("Sensor: " + Integer.toString(update));		
	}
}