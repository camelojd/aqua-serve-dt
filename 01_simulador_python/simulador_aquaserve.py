"""
Simulador de datos - Proyecto AquaServe
Gemelo Digital de captacion pluvial + enfriamiento DLC para centros de datos.

Publica un JSON unico cada segundo al topico solarpunk/aquaserve/estado
con presion, caudal, nivel_tanque, turbidez y timestamp, con variacion
suave tipo onda seno + ruido.
"""

import time
import math
import json
import random

import paho.mqtt.client as mqtt

BROKER = "broker.emqx.io"
PORT = 1883
CLIENT_ID = "aquaserve_simulador"

TOPIC_ESTADO = "solarpunk/aquaserve/estado"

# --- Rangos de las variables ---
PRESION_MIN, PRESION_MAX = 20.0, 60.0        # PSI (alerta si baja)
CAUDAL_MIN, CAUDAL_MAX = 0.0, 5.0            # L/min por bajante (doc: 2-5 nominal)
NIVEL_MIN, NIVEL_MAX = 10.0, 100.0           # % deposito (alarma <20%)
TURBIDEZ_MIN, TURBIDEZ_MAX = 0.2, 0.9        # NTU (objetivo <1)

INTERVALO_SEGUNDOS = 1


def onda_suave(t, minimo, maximo, periodo_segundos, ruido=0.0):
    """Onda seno acotada + ruido, imitando un sensor real."""
    amplitud = (maximo - minimo) / 2
    centro = minimo + amplitud
    valor = centro + amplitud * math.sin(2 * math.pi * t / periodo_segundos)
    valor += random.uniform(-ruido, ruido)
    return max(minimo, min(maximo, valor))


def main():
    client = mqtt.Client(callback_api_version=mqtt.CallbackAPIVersion.VERSION2, client_id=CLIENT_ID)
    client.connect(BROKER, PORT, keepalive=60)
    client.loop_start()

    print(f"Conectado a {BROKER}:{PORT}. Publicando JSON en {TOPIC_ESTADO} ...")

    t = 0
    try:
        while True:
            presion = onda_suave(t, PRESION_MIN, PRESION_MAX, periodo_segundos=300, ruido=0.5)
            caudal = onda_suave(t, CAUDAL_MIN, CAUDAL_MAX, periodo_segundos=180, ruido=0.1)
            nivel_tanque = onda_suave(t, NIVEL_MIN, NIVEL_MAX, periodo_segundos=600, ruido=1.0)
            turbidez = onda_suave(t, TURBIDEZ_MIN, TURBIDEZ_MAX, periodo_segundos=240, ruido=0.02)

            payload = {
                "presion": round(presion, 2),
                "caudal": round(caudal, 2),
                "nivel_tanque": round(nivel_tanque, 2),
                "turbidez": round(turbidez, 2),
                "timestamp": int(time.time()),
            }

            client.publish(TOPIC_ESTADO, json.dumps(payload))

            print(
                f"[t={t:04d}s] Presion={presion:.2f}PSI  Caudal={caudal:.2f}L/min  "
                f"Nivel={nivel_tanque:.2f}%  Turbidez={turbidez:.2f}NTU"
            )

            t += INTERVALO_SEGUNDOS
            time.sleep(INTERVALO_SEGUNDOS)
    except KeyboardInterrupt:
        print("\nSimulador detenido por el usuario.")
    finally:
        client.loop_stop()
        client.disconnect()


if __name__ == "__main__":
    main()
