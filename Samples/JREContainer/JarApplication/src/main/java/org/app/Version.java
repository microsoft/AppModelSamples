package org.app;

import javax.swing.*;

class Version {

    private static void createAndShowGUI() {
        //Make sure we have nice window decorations.
        JFrame.setDefaultLookAndFeelDecorated(true);
 
        //Create and set up the window.
        JFrame frame = new JFrame("Test");
        frame.setSize(200, 100);
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
 
        //Show Java version that is running the app
        JLabel label = new JLabel("Version: " + System.getProperty("java.version"), SwingConstants.CENTER);
        frame.getContentPane().add(label);
 
        //Display the window.
        frame.setVisible(true);
    }

    public static void main(String[] args) {
        SwingUtilities.invokeLater(Version::createAndShowGUI);
    }
}