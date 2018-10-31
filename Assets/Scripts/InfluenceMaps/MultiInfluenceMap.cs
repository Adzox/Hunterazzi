using UnityEngine;
using System.Collections;

public class MultiInfluenceMap : MonoBehaviour {

    [Header("Map Settings")]
    public SharedGrid grid;
    public ObstacleHeightMap obstacleHeights;
    public float updateFrequency = 30;
    private float updateTime;
    public Renderer mapRenderer;

    [Header("Sight Settings")]
    public float sightDecay = 1;
    public float visualizedMaxPresence; 

    [Header("Smell Settings")]
    public float smellDecay = 1;
    public float visualizedMaxSmell;

    [Header("Sound Settings")]
    public float soundDecay = 1;
    public float visualizedMaxSound;

    private InfluenceMap presence;
    private InfluenceMap smell;
    private InfluenceMap sound;

    public delegate void OnUpdate();
    public OnUpdate afterMapUpdate;

    private Texture2D tex;
    private Color[] clearColors;

    private void Awake() {
        var maps = new GameObject("InfluenceMaps");
        maps.transform.SetParent(gameObject.transform);
        updateTime = 1 / updateFrequency;

        presence = maps.AddComponent<InfluenceMap>();
        presence.grid = grid;
        presence.obstacleHeights = obstacleHeights;
        presence.updateFrequency = updateFrequency;
        presence.decayPerSecond = sightDecay;
        presence.visualizedMaxValue = visualizedMaxPresence;
        presence.standalone = false;

        smell = maps.AddComponent<InfluenceMap>();
        smell.grid = grid;
        smell.obstacleHeights = obstacleHeights;
        smell.updateFrequency = updateFrequency;
        smell.decayPerSecond = smellDecay;
        smell.visualizedMaxValue = visualizedMaxSmell;
        smell.standalone = false;

        sound = maps.AddComponent<InfluenceMap>();
        sound.grid = grid;
        sound.obstacleHeights = obstacleHeights;
        sound.updateFrequency = updateFrequency;
        sound.decayPerSecond = soundDecay;
        sound.visualizedMaxValue = visualizedMaxSound;
        sound.standalone = false;
    }

    private void Start() {
        tex = new Texture2D(grid.GetWidth(), grid.GetHeight());
        clearColors = new Color[grid.GetWidth() * grid.GetHeight()];
        if (mapRenderer != null)
            mapRenderer.material.mainTexture = tex;

        StartCoroutine("UpdateMultiMapParallel");
    }

    public void AddMultiSource(MultiSource source) {
        source.presence.parentMap = presence;
        source.smell.parentMap = smell;
        source.sound.parentMap = sound;
    }

    public void RemoveMultiSource(MultiSource source) {
        presence.RemoveInfluenceSource(source.presence);
        smell.RemoveInfluenceSource(source.smell);
        sound.RemoveInfluenceSource(source.sound);
        source.presence.parentMap = null;
        source.smell.parentMap = null;
        source.sound.parentMap = null;
    }

    public void AddPresence(Vector3 point, float val) {
        presence.AddInfluence(val, grid.WorldToGrid(point));
    }

    public void AddSmell(Vector3 point, float val) {
        smell.AddInfluence(val, grid.WorldToGrid(point));
    }

    public void AddSound(Vector3 point, float val) {
        sound.AddInfluence(val, grid.WorldToGrid(point));
    }

    public float GetInfluence(Vector2Int pos, float sightMod = 1, float smellMod = 1, float soundMod = 1) {
        if (!grid.InBounds(pos))
            return 0;

        return presence.GetInfluence(pos.x, pos.y) * sightMod + smell.GetInfluence(pos.x, pos.y) * smellMod + sound.GetInfluence(pos.x, pos.y) * soundMod;
    }

    public float GetInfluence(Vector3 point, float sightMod = 1, float smellMod = 1, float soundMod = 1) {
        Vector2Int pos = grid.WorldToGrid(point);
        return GetInfluence(pos, sightMod, smellMod, soundMod);
    }

    public IEnumerator UpdateMultiMapParallel() {
        yield return new WaitForSecondsRealtime(0.5f);
        while (true) {
            yield return this.WaitForAllParams(presence.UpdateInfluencesParallel(), smell.UpdateInfluencesParallel(), sound.UpdateInfluencesParallel());

            if (afterMapUpdate != null)
                afterMapUpdate.Invoke();

            presence.Display();
            smell.Display();
            sound.Display();

            // Clear and blend!
            tex.SetPixels(clearColors);
            tex.Add(presence.tex, 1f); // Fully add presence map!
            tex.Add(smell.tex, 0.5f);
            tex.Add(sound.tex, 0.5f);
            tex.Apply();

            yield return new WaitForSeconds(updateTime);
        }
    }
}
