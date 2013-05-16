/*
 * urlLaunch.java
 *
 * This application launches a specific URL using the device's
 * default browser. Refer to BlackBerry Developer Knowledge 
 * Base article DB-00701 for information on how to launch a
 * particular browser.
 */

package com.bbdevfundamentals.urlLaunch;

import net.rim.blackberry.api.browser.Browser;
import net.rim.blackberry.api.browser.BrowserSession;
import net.rim.device.api.system.Application;

public class urlLaunch extends Application {

        // Put the URL you want launched here
        private static String appURL = "http://www.bbdevfundamentals.com";

        public static void main(String[] args) {
                urlLaunch theApp = new urlLaunch();
                theApp.enterEventDispatcher();
        }

        public urlLaunch() {
                // Get the default browser session
                BrowserSession browserSession = Browser.getDefaultSession();
                // Then display the page using the browser session
                browserSession.displayPage(appURL);
                // Once the URL is launched, close this application
                System.exit(0);
        }
}
