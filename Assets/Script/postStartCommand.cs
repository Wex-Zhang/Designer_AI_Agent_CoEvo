using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class PostStartCommand : MonoBehaviour
{
    // Button reference
    public Button sendPostButton;
    
    // IP address and port of the server
    public string serverIP = "127.0.0.1";  // Change this to your server's IP
    public int serverPort = 3000;          // Change this to your server's port

    void Start()
    {
        // Attach the button click listener
        sendPostButton.onClick.AddListener(OnSendPostButtonClick);
    }

    // Button click event
    void OnSendPostButtonClick()
    {
        // JSON data to be sent
        string jsonData = "{\"command\":\"start\"}";

        // Send the JSON data via UDP
        SendUdpRequest(serverIP, serverPort, jsonData);
    }

    // Function to send data via UDP
    void SendUdpRequest(string ipAddress, int port, string jsonData)
    {
        try
        {
            // Convert the JSON string to bytes
            byte[] data = Encoding.UTF8.GetBytes(jsonData);

            // Create a UDP client
            UdpClient udpClient = new UdpClient();

            // Send the data to the specified IP and port
            udpClient.Send(data, data.Length, ipAddress, port);

            Debug.Log("Data sent successfully via UDP.");
        }
        catch (SocketException ex)
        {
            Debug.LogError("UDP send failed: " + ex.Message);
        }
    }
}
