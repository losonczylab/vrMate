using UnityEngine;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Communicator {

    private ConcurrentQueue<String> messageQueue;
    private UdpClient client;
    private Thread receiveThread;

    public Communicator(int port) {
        client = new UdpClient(port);
		client.Client.ReceiveTimeout = 10;
        messageQueue = new ConcurrentQueue<String>();
        receiveThread = new Thread(new ThreadStart(receiveDataThreaded));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    public void receiveData() {
		IPEndPoint RemoteIpEndPoint = new IPEndPoint (IPAddress.Any, 0);
        for (int i = 0; ((i < 5) && (client.Available > 0)); i++) {
            String returnData = "";
            try{
                Byte[] receiveBytes = client.Receive(ref RemoteIpEndPoint);
                returnData = Encoding.ASCII.GetString(receiveBytes);
            } catch (Exception e) {
                Debug.Log(e.ToString ());
            }
            messageQueue.Enqueue(returnData);
        }
    }

    void receiveDataThreaded() {
		IPEndPoint RemoteIpEndPoint = new IPEndPoint (IPAddress.Any, 0);

		while (true) {
            if (client.Available > 0) {
                String returnData = "";
                try{
                    Byte[] receiveBytes = client.Receive(ref RemoteIpEndPoint);
                    returnData = Encoding.ASCII.GetString(receiveBytes);
                } catch (Exception e) {
                    Debug.Log(e.ToString ());
                }
                //Debug.Log(returnData);
                messageQueue.Enqueue(returnData);
            }
        }
    }

    public String checkMessage() {
        String result;
        if (messageQueue.TryDequeue(out result)) {
            return result;
        }

        return null;
    }
}
