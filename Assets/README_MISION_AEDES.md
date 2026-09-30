# Checklist final — Misión Aedes: Pangoa

## Estado (marcar solo después de probar)

- [ ] Zona 1 funcional: inicio automático con objetivos/tiempo confirmados.
- [ ] Zona 2 funcional: 6 objetivos, 345 segundos, reducción de 15 por objeto.
- [ ] Zona 3 funcional: trigger, panel, objetivos y tiempo configurados.
- [ ] Progresión zonas: un ProgresoZonas compartido; índices 1/2/3.
- [ ] HUD y animaciones revisados.
- [ ] Decisiones: lectura pausa tiempo; acierto visible 4 segundos.
- [ ] Interactuables con índice de zona asignado (0 es compatibilidad sin pertenencia).
- [ ] Colliders ajustados a las bases y límites comprobados.
- [ ] Sorting Y: Player y objetos ordenables comparten Sorting Layer.
- [ ] PanelVictoria: resultados y Continuar conectados.
- [ ] PanelDerrota: motivo y Reiniciar conectados.
- [ ] Pausa: ESC no compite con decisiones/diálogos.
- [ ] Audio.
- [ ] Decoración.
- [ ] Pruebas completas y Console sin excepciones.
- [ ] Build probado en el dispositivo objetivo.

## Orden recomendado para terminar

1. Probar Zona 2.
2. Configurar progreso.
3. Preparar Zona 1.
4. Crear Zona 3.
5. Añadir interactuables Zona 3.
6. Aplicar colliders/sorting.
7. Pulido visual.
8. Audio.
9. Auditoría final (`Mision Aedes > Auditoria final`, solo lectura).
10. Build.

## Conexiones esenciales

- GameManager: asignar ProgresoZonas. Continuar no inicia la siguiente zona.
- Zona 1: ZonaConfigurable, índice 1, Iniciar Automaticamente activado, Player y progreso asignados; panel opcional.
- Zona 2: conservar TriggerZona2 e InicioZona2UI, ambos índice 2; asignar gestor/progreso al trigger. No añadir otro inicio paralelo.
- Zona 3: ZonaConfigurable, índice 3, trigger y panel; botón → ComenzarZona().
- MenuPausaUI: en un objeto siempre activo fuera de PanelPausa; botones → Reanudar() / ReiniciarZona().
- El reinicio existente recarga la escena completa y reinicia el progreso de la partida; no hay guardado.
- Visuales opcionales: PanelUIAnimator en paneles, TMPPulseOnChange en textos, ZonaBannerUI con dos TMP, ObjetoResueltoVisual con sprite y objeto interactuable asignados.
- Las herramientas de Editor se aplican manualmente a selección y soportan Undo; no se ejecutan al importar.
