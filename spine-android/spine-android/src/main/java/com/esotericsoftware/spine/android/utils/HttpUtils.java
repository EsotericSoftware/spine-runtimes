package com.esotericsoftware.spine.android.utils;

import android.os.Build;

import java.io.BufferedInputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.file.Files;

/**
 * Helper to load http resources.
 */
public class HttpUtils {
    /**
     * Download a file from an url into a target directory. It keeps the name from the {@code url}.
     * This should NOT be executed on the main run loop.
     */
    public static File downloadFrom(URL url, File targetDirectory) throws RuntimeException {
        HttpURLConnection urlConnection = null;
        InputStream inputStream = null;
        OutputStream outputStream = null;

        try {
            urlConnection = (HttpURLConnection) url.openConnection();
            urlConnection.connect();

            if (urlConnection.getResponseCode() != HttpURLConnection.HTTP_OK) {
                throw new RuntimeException("Failed to connect: HTTP response code " + urlConnection.getResponseCode());
            }

            inputStream = new BufferedInputStream(urlConnection.getInputStream());

            String atlasUrlPath = url.getPath();
            String fileName = atlasUrlPath.substring(atlasUrlPath.lastIndexOf('/') + 1);
            File file = new File(targetDirectory, fileName);

            // Create an OutputStream to write to the file
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                outputStream = Files.newOutputStream(file.toPath());
            } else {
                //noinspection IOStreamConstructor
                outputStream = new FileOutputStream(file);
            }

            byte[] buffer = new byte[1024];
            int bytesRead;

            // Write the input stream to the output stream
            while ((bytesRead = inputStream.read(buffer)) != -1) {
                outputStream.write(buffer, 0, bytesRead);
            }
            return file;
        } catch (IOException e) {
            throw new RuntimeException(e);
        } finally {
            if (outputStream != null) {
                try {
                    outputStream.flush();
                    outputStream.close();
                } catch (IOException e) {
                    // Nothing we can do
                }
            }

            if (inputStream != null) {
                try {
                    inputStream.close();
                } catch (IOException e) {
                    // Nothing we can do
                }
            }

            if (urlConnection != null) {
                urlConnection.disconnect();
            }
        }
    }
}

