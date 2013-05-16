package com.bbdevfundamentals.SensorTest;

import net.rim.device.api.ui.UiApplication;
import net.rim.device.api.ui.component.Dialog;
import net.rim.device.api.ui.component.LabelField;
import net.rim.device.api.ui.container.MainScreen;
import net.rim.device.api.system.Application;
import net.rim.device.api.system.SensorListener;
import net.rim.device.api.system.Sensor;

//Create the HelloWorld class
public class SensorTest extends UiApplication implements SensorListener {

	public static void main(String[] args) {
		// Instantiate the HelloWorld object
		SensorTest st = new SensorTest();
		// Enter the Event Dispatcher
		st.enterEventDispatcher();
	}

	public SensorTest() {
		// Create the HelloWorldScreen and open it
		pushScreen(new SensorTestScreen());
		Sensor.addListener(Application.getApplication(), this, Sensor.HOLSTER);
	}

	public void onSensorUpdate(int sensorId, int update) {
		if (update == Sensor.STATE_IN_HOLSTER) {
			Dialog.alert("Device is holstered");				
		} else {
			Dialog.alert("Device is not holstered");
		}
	}
}

final class SensorTestScreen extends MainScreen {

	public SensorTestScreen() {
		super();
		// Create the screen's title
		LabelField lblTitle = new LabelField("Sensor Test", LabelField.ELLIPSIS
				| LabelField.USE_ALL_WIDTH);
		setTitle(lblTitle);
	}

}
