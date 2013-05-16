package com.bbdevfundamentals.helloworld;

import net.rim.device.api.ui.UiApplication;
import net.rim.device.api.ui.component.LabelField;
import net.rim.device.api.ui.container.MainScreen;

//Create the HelloWorld class
public class HelloWorld extends UiApplication {

	public static void main(String[] args) {
		// Instantiate the HelloWorld object
		HelloWorld hw = new HelloWorld();
		// Enter the Event Dispatcher
		hw.enterEventDispatcher();
	}

	public HelloWorld() {
		// Create the HelloWorldScreen and open it
		pushScreen(new HelloWorldScreen());		
	}
}

final class HelloWorldScreen extends MainScreen {
	public HelloWorldScreen() {
		super();
		// Create the screen's title
		LabelField lblTitle = new LabelField(
				"BlackBerry Development Fundamentals", LabelField.ELLIPSIS
						| LabelField.USE_ALL_WIDTH);
		setTitle(lblTitle);
		// add the text to the screen
		add(new LabelField("Hello readers!"));
	}
}
