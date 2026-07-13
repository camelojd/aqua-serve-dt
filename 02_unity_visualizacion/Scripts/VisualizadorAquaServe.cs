/*
 * VisualizadorAquaServe.cs
 * Gemelo Digital - Proyecto AquaServe
 *
 * Cliente MQTT basado en M2MqttUnity. Se suscribe al topico unico
 * solarpunk/aquaserve/estado, parsea el JSON y actualiza color y UI:
 *   VERDE    -> todo en rango
 *   AMARILLO -> presion baja (< umbral)
 *   ROJO     -> caudal critico (< umbral) o nivel de tanque < 20%
 */

using System;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;

[Serializable]
public class EstadoAquaServe
{
    public float presion;
    public float caudal;
    public float nivel_tanque;
    public float turbidez;
    public long timestamp;
}

public class VisualizadorAquaServe : M2MqttUnityClient
{
    private const string TOPIC_ESTADO = "solarpunk/aquaserve/estado";

    [Header("Umbrales de alerta")]
    public float presionBaja = 30.0f;    // PSI: amarillo por debajo
    public float caudalCritico = 1.0f;   // L/min: rojo por debajo
    public float nivelCritico = 20.0f;   // %: rojo por debajo

    [Header("Referencias de escena")]
    public Renderer objetoIndicador;
    public Text textoPresion;
    public Text textoCaudal;
    public Text textoNivelTanque;
    public Text textoTurbidez;

    private EstadoAquaServe estado = new EstadoAquaServe();

    protected override void Start()
    {
        brokerAddress = "broker.emqx.io";
        brokerPort = 1883;
        autoConnect = true;
        base.Start();
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(
            new string[] { TOPIC_ESTADO },
            new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        Debug.Log("VisualizadorAquaServe: suscrito a " + TOPIC_ESTADO);
    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(new string[] { TOPIC_ESTADO });
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        if (topic != TOPIC_ESTADO)
        {
            return;
        }

        string json = System.Text.Encoding.UTF8.GetString(message);
        try
        {
            estado = JsonUtility.FromJson<EstadoAquaServe>(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("VisualizadorAquaServe: JSON invalido: " + ex.Message);
        }
    }

    protected override void Update()
    {
        base.Update();
        ActualizarColorEstado();
        ActualizarTextosUI();
    }

    private void ActualizarColorEstado()
    {
        if (objetoIndicador == null)
        {
            return;
        }

        Color colorEstado;

        if (estado.caudal < caudalCritico || estado.nivel_tanque < nivelCritico)
        {
            colorEstado = Color.red;
        }
        else if (estado.presion < presionBaja)
        {
            colorEstado = Color.yellow;
        }
        else
        {
            colorEstado = Color.green;
        }

        objetoIndicador.material.color = colorEstado;
    }

    private void ActualizarTextosUI()
    {
        if (textoPresion != null) textoPresion.text = $"Presion: {estado.presion:F1} PSI";
        if (textoCaudal != null) textoCaudal.text = $"Caudal: {estado.caudal:F2} L/min";
        if (textoNivelTanque != null) textoNivelTanque.text = $"Nivel tanque: {estado.nivel_tanque:F0} %";
        if (textoTurbidez != null) textoTurbidez.text = $"Turbidez: {estado.turbidez:F2} NTU";
    }
}
