using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIController : MonoBehaviour
{
    // Tiempo entre movimientos del bot
    public float turnTime = 0.5f;

    // Estado del bot
    private bool botActive = false;
    private Coroutine playRoutine = null;

    // No se inicia automáticamente. Usar StartBot / ToggleBot desde UI.
    void Start()
    {
        // Intencionalmente vacío para controlar el inicio desde UI.
    }

    // Método público para el botón UI: alterna el estado del bot
    public void ToggleBot()
    {
        SetBotActive(!botActive);
    }

    // Método público para fijar estado explícito (útil desde el Inspector Button con parámetro booleano si se desea)
    public void SetBotActive(bool active)
    {
        botActive = active;
        if (botActive)
            StartBot();
        else
            StopBot();
    }

    // Inicia la corrutina si no está ya en marcha
    public void StartBot()
    {
        if (playRoutine == null)
            playRoutine = StartCoroutine(Play());
    }

    // Para la corrutina si está en marcha
    public void StopBot()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }
    }

    IEnumerator Play()
    {
        yield return new WaitForSeconds(1f);

        while (botActive && !GameManager.instance.endGame)
        {
            bool actionDone = LogicPlay();
            if (!actionDone)
            {
                // Si no hay lógica aplicable, jugar aleatoriamente
                RandomPlay();
            }

            yield return new WaitForSeconds(turnTime);
        }

        playRoutine = null;
    }

    // Lógica general del bot basada en reglas básicas de Minesweeper
    bool LogicPlay()
    {
        bool action = false;

        // Recolectamos todas las piezas de la escena
        Piece[] allPieces = FindObjectsOfType<Piece>();

        // Filtramos casillas comprobadas
        var checkedPieces = allPieces.Where(p => p.isCheck()).ToList();

        foreach (var cell in checkedPieces)
        {
            // Obtener número de bombas alrededor usando Generator (misma API que Piece)
            int bombsAround = Generator.gen.GetBombsAround(cell.getX(), cell.getY());
            if (bombsAround == 0) continue;

            // Vecinos alrededor (8 direcciones)
            var neighbors = allPieces.Where(p =>
            {
                int dx = Mathf.Abs(p.getX() - cell.getX());
                int dy = Mathf.Abs(p.getY() - cell.getY());
                return !(dx == 0 && dy == 0) && dx <= 1 && dy <= 1;
            }).ToList();

            // Contamos flags ya colocadas y ocultas (no check y no flagged)
            int flaggedNeighbors = neighbors.Count(n => n.isFlagged());
            var hiddenNeighbors = neighbors.Where(n => !n.isCheck() && !n.isFlagged()).ToList();

            // Regla 1: si bombsAround == flaggedNeighbors + hiddenNeighbors.Count => todas las ocultas son minas -> poner banderas
            if (hiddenNeighbors.Count > 0 && bombsAround == flaggedNeighbors + hiddenNeighbors.Count)
            {
                foreach (var h in hiddenNeighbors)
                {
                    if (GameManager.instance.flagsRemaining <= 0) break;
                    // Colocar bandera visual y notificar a GameManager
                    h.DrawFlag();
                    GameManager.instance.FlagPlaced(h.isBomb());
                    action = true;
                }
                if (action) return true;
            }

            // Regla 2: si bombsAround == flaggedNeighbors => todas las ocultas son seguras -> abrirlas
            if (hiddenNeighbors.Count > 0 && bombsAround == flaggedNeighbors)
            {
                foreach (var h in hiddenNeighbors)
                {
                    // Abrir la casilla (esto llamará a la lógica existente que marca check y revela)
                    h.DrawBomb();
                    action = true;

                    // Si se ha terminado el juego al abrir, salimos
                    if (GameManager.instance.endGame) return true;
                }
                if (action) return true;
            }
        }

        return action;
    }

    void RandomPlay()
    {
        // Seleccionamos aleatoriamente una casilla que no esté comprobada ni tenga bandera
        Piece[] allPieces = FindObjectsOfType<Piece>();
        var candidates = allPieces.Where(p => !p.isCheck() && !p.isFlagged()).ToArray();
        if (candidates.Length == 0) return;

        var pick = candidates[Random.Range(0, candidates.Length)];
        pick.DrawBomb();
    }
}