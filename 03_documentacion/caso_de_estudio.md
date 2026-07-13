# Caso de Estudio: Gemelo Digital para Enfriamiento Líquido de Centros de Datos con Captación Pluvial

**Proyecto:** AquaServe
**Dominio:** Infraestructura de datos / Gestión hídrica

---

## Desafío

Los centros de datos para IA se toman cantidades enormes de agua y energía solo en enfriarse. El enfoque Solarpunk plantea algo bonito: captar agua lluvia en edificios de 5 pisos o más, generar electricidad con microturbinas helicoidales en los bajantes, filtrar esa agua y usarla como refrigerante en enfriamiento líquido directo (DLC). Pero operar algo así obliga a vigilar en tiempo real la presión, el caudal, el nivel del depósito y la calidad del agua (turbidez < 1 NTU); porque si el caudal cae o el depósito baja del 20%, los servidores se quedan sin refrigeración.

## Solución

Lo que hice fue un **gemelo digital** que replica todo el ciclo hidráulico:

- Escribí un **simulador en Python** que publica cada segundo un JSON con presión, caudal, nivel de tanque y turbidez al tópico MQTT `solarpunk/aquaserve/estado`.
- Del otro lado tengo una escena de **Unity** (con el cliente M2MqttUnity) que lee ese JSON y pinta el estado con código de colores: **verde** (todo normal), **amarillo** (presión baja) y **rojo** (caudal crítico o depósito < 20%), más un panel con los valores en vivo.

## Tecnologías

- **Unity** (C#, uGUI) con **M2MqttUnity**
- **Python** (paho-mqtt)
- **MQTT** (broker `broker.emqx.io`) con mensajes **JSON**

## Resultados

- Conseguí monitorear en remoto y en tiempo real todo el circuito de agua y refrigeración.
- Las alertas visuales me avisan solas cuando el caudal está crítico o el depósito bajo; que en el sistema real serían justo las condiciones que activan el cambio a agua de red y el corte protector del DLC.
- Y lo dejé como una arquitectura que puedo extender hacia KPIs industriales (PUE ≤ 1.15, potencia generada por las turbinas) y hasta control bidireccional, con un comando "simular lluvia" que mando desde Unity.
