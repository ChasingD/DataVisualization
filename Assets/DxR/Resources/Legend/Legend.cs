using System;
using System.Collections;
using System.Collections.Generic;
using DxR;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI; // 使用Unity的UI系统进行布局

public class Legend : MonoBehaviour
{

    LineRenderer colorLine = null;
    private Interactions interactionsObject = null;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateSpecs(JSONNode legendSpecs, ref DxR.ChannelEncoding channelEncoding, GameObject markPrefab)
    {
        // Create title:
        if (legendSpecs["title"] != null)
        {
            gameObject.GetComponent<Legend>().SetTitle(legendSpecs["title"].Value);
        }

        if (legendSpecs["type"] == "symbol")
        {
            // Create symbols:
            ConstructSymbols(legendSpecs, ref channelEncoding, markPrefab);

            if (legendSpecs["filter"].AsBool && interactionsObject != null)
            {
                interactionsObject.EnableLegendToggleFilter(gameObject);
            }

        }
        else if (legendSpecs["type"] == "gradient")
        {
            ConstructGradient(legendSpecs, ref channelEncoding);
        }

        // Orient legend:
        if (legendSpecs["orient"] != null && legendSpecs["face"] != null)
        {
            if (legendSpecs["x"] != null && legendSpecs["y"] != null && legendSpecs["z"] != null)
            {
                gameObject.GetComponent<Legend>().SetOrientation(legendSpecs["orient"].Value, legendSpecs["face"].Value,
                    legendSpecs["x"].AsFloat, legendSpecs["y"].AsFloat, legendSpecs["z"].AsFloat);
            }
            else
            {
                gameObject.GetComponent<Legend>().SetOrientation(legendSpecs["orient"].Value, legendSpecs["face"].Value,
                    0, 0, 0);
            }
        }
        else
        {
            throw new Exception("Legend requires both orient and face specs.");
        }
    }

    private void ConstructGradient(JSONNode legendSpecs, ref ChannelEncoding channelEncoding)
    {
        colorLine = gameObject.GetComponentInChildren<LineRenderer>(true);

        bool addTicks = false;
        Transform ticks = gameObject.transform.Find("Ticks");
        GameObject tickPrefab = null;
        if (ticks != null)
        {
            addTicks = true;
            ticks.gameObject.SetActive(true);
            tickPrefab = Resources.Load("Legend/LegendTick") as GameObject;
        }

        if (colorLine == null)
        {
            throw new Exception("Cannot find ColorLine LineRenderer object in legend.");
        }

        colorLine.gameObject.SetActive(true);
        colorLine.material = new Material(Shader.Find("Sprites/Default"));

        float width = 0.2f;
        float height = 0.05f;
        if (legendSpecs["gradientWidth"] != null && legendSpecs["gradientHeight"] != null)
        {
            width = legendSpecs["gradientWidth"].AsFloat * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
            height = legendSpecs["gradientHeight"].AsFloat * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
        }

        List<Vector3> positionsList = new List<Vector3>();
        List<GradientColorKey> colorKeyList = new List<GradientColorKey>();
        List<GradientAlphaKey> alphaKeyList = new List<GradientAlphaKey>();

        float alpha = 1.0f;
        int domainCount = channelEncoding.scale.domain.Count;
        for (int i = 0; i < domainCount; i++)
        {
            float pct = channelEncoding.scale.GetDomainPct(channelEncoding.scale.domain[i]);
            positionsList.Add(new Vector3(width * pct, 0.0f, 0.0f));
            Color col;
            ColorUtility.TryParseHtmlString(channelEncoding.scale.range[i], out col);
            colorKeyList.Add(new GradientColorKey(col, pct));
            alphaKeyList.Add(new GradientAlphaKey(alpha, pct));

            if (addTicks && tickPrefab != null)
            {
                GameObject tick = Instantiate(tickPrefab, ticks.transform.position, ticks.transform.rotation, ticks.transform);

                Vector3 pos = Vector3.zero;
                pos.x = width * pct;
                pos.y = 0.04f;             // TODO: Get this from text size.
                tick.transform.Translate(pos);

                tick.GetComponent<TextMesh>().text = channelEncoding.scale.domain[i];
            }
        }

        colorLine.positionCount = positionsList.Count;
        colorLine.SetPositions(positionsList.ToArray());
        colorLine.startWidth = height;
        colorLine.endWidth = height;

        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeyList.ToArray(), alphaKeyList.ToArray());
        colorLine.colorGradient = gradient;

        colorLine.transform.parent = gameObject.transform;

        // 将 HoloToolkit 的 ObjectCollection 替换为 Unity 的布局系统
        GridLayoutGroup layoutGroup = gameObject.GetComponent<GridLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = gameObject.AddComponent<GridLayoutGroup>();
            layoutGroup.cellSize = new Vector2(0.08f, 0.08f); // 设置默认单元格大小
            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.constraintCount = 3; // 设置固定列数为 3
        }
    }

    private void ConstructSymbols(JSONNode legendSpecs, ref ChannelEncoding channelEncoding, GameObject markPrefab)
    {
        GameObject legendValuePrefab = Resources.Load("Legend/LegendValue", typeof(GameObject)) as GameObject;
        if (channelEncoding.channel == "color")
        {
            foreach (string domainValue in channelEncoding.scale.domain)
            {
                // Create container for mark + label:
                GameObject legendValueInstance = Instantiate(legendValuePrefab, gameObject.transform.position,
                        gameObject.transform.rotation, gameObject.transform);

                // Create mark instance:
                GameObject markInstance = Instantiate(markPrefab, legendValueInstance.transform.position,
                        legendValueInstance.transform.rotation, legendValueInstance.transform);

                // Apply channel value for this domain:
                string channelValue = channelEncoding.scale.ApplyScale(domainValue);
                Mark markComponent = markInstance.GetComponent<Mark>();
                markComponent.SetChannelValue(channelEncoding.channel, channelValue);

                // Set size:
                markComponent.SetChannelValue("size", "20");

                // Assign mark and label:
                legendValueInstance.GetComponent<LegendValue>().SetDataFieldName(channelEncoding.field);
                legendValueInstance.GetComponent<LegendValue>().SetTitle(domainValue);
                legendValueInstance.GetComponent<LegendValue>().SetMark(markInstance);

                // 使用 Unity 的布局系统来替代 ObjectCollection
                GridLayoutGroup layoutGroup = gameObject.GetComponent<GridLayoutGroup>();
                if (layoutGroup == null)
                {
                    layoutGroup = gameObject.AddComponent<GridLayoutGroup>();
                    layoutGroup.cellSize = new Vector2(0.05f, 0.05f); // 设置单元格高度
                    layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    layoutGroup.constraintCount = 1; // 设置为一列
                }
            }
        }
        else if (channelEncoding.channel == "opacity")
        {
            // TODO: 实现 opacity 渠道的图例构造
        }
        else if (channelEncoding.channel == "size")
        {
            // TODO: 实现 size 渠道的图例构造
        }
        else if (channelEncoding.channel == "shape")
        {
            // TODO: 实现 shape 渠道的图例构造
        }
        else
        {
            throw new Exception("Legend constructor does not know how to construct legend for " + channelEncoding.channel);
        }
    }

    public void SetTitle(string title)
    {
        gameObject.GetComponentInChildren<TextMesh>().text = title;
    }

    internal void SetOrientation(string orient, string face, float x, float y, float z)
    {
        gameObject.GetComponentInChildren<TextMesh>().anchor = TextAnchor.UpperLeft;
        gameObject.transform.localPosition = new Vector3(x, y, z) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
    }

    internal void Init(Interactions interactions)
    {
        interactionsObject = interactions;
    }
}
