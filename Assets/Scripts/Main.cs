using EX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    [Min(2)]
    public int numColors;
    [Range(0, 1)]
    public float startHue;

    [Min(0)]
    public int colorCheck;

    float maxSize = 1.25f;
    float boundingBoxRadius = 4.5f;
    public float offset = .25f;

    public bool reverseOrder;

    public enum MixMode { Multiply, Add }
    public MixMode mixMode;

    [Header("UI")]
    public Slider numColorsSlider;
    public Text numColorsText;
    public Slider startHueSlider;
    public Text hueShiftText;
    public Slider colorCheckSlider;
    public Image colorCheckHandle;
    public Toggle reverseOrderToggle;
    public Button quitButton;
    public Dropdown mixModeDropdown;

    private void Awake()
    {
        Application.targetFrameRate = 30;
    }

    private void Start()
    {
        reverseOrderToggle.onValueChanged.AddListener(delegate { UpdateColors(); });
        numColorsSlider.onValueChanged.AddListener(delegate { UpdateColors(); });
        startHueSlider.onValueChanged.AddListener(delegate { UpdateColors(); });
        colorCheckSlider.onValueChanged.AddListener(delegate { UpdateColors(); });
        mixModeDropdown.onValueChanged.AddListener(delegate { UpdateColors(); });
        quitButton.onClick.AddListener(delegate { Application.Quit(); });
        UpdateColors();
    }

    public void SetBounds(float size, float off)
    {
        if (boundingBoxRadius != size || offset != off)
        {
            boundingBoxRadius = size;
            offset = off;
            UpdateColors();
        }
    }

    //private void Update() => UpdateColors();

    void UpdateColors()
    {
        GLGizmos.ClearDrawActions();
        numColors = (int)numColorsSlider.value;
        numColorsText.text = numColorsSlider.value.ToString();
        startHue = (float)startHueSlider.value / 360;
        hueShiftText.text = startHueSlider.value.ToString();
        reverseOrder = reverseOrderToggle.isOn;
        mixMode = (MixMode)mixModeDropdown.value;

        colorCheckSlider.maxValue = numColors;
        colorCheck = (int)colorCheckSlider.value;
        float handleHue = startHue + ((float)colorCheck / (float)numColors);
        float handleHueOverflow = (int)handleHue;
        handleHue = handleHue - handleHueOverflow;
        colorCheckHandle.color = Color.HSVToRGB(handleHue, colorCheck == numColors ? 0 : 1, 1);

        float r = ((boundingBoxRadius - offset) * Mathf.PI) / (numColors + Mathf.PI);
        r *= .95f;
        r = Mathf.Clamp(r, 0, maxSize);
        float R = (boundingBoxRadius - offset) - r;
        float rM = r * .33f;

        float angle = 0;
        for (float i = 0; i < 1; i += (1f/numColors))
        {
            angle = i * 360;
            //angle = (i + startHue) * 360 - (((int)(((i + startHue) * 360) / 360)) * 360);
            float hue = i + startHue;
            float hueOverflow = (int)(i + startHue);
            hue = hue - hueOverflow;

            Color c = Color.HSVToRGB(hue, 1, 1);
            GLGizmos.SetColor(c);

            Vector2 pos = (Vector2.up * R).Rotate((reverseOrder ? 1 : -1) * angle) + (Vector2.up  * offset);
            GLGizmos.DrawSolidCircle(pos, r);
        }

        int num = 0;
        for (float i = 0; i < 1; i += (1f / numColors))
        {
            angle = i * 360;
            float hue = i + startHue;
            float hueOverflow = (int)(i + startHue);
            hue = hue - hueOverflow;

            Color c = Color.HSVToRGB(hue, 1, 1);

            Vector2 pos = (Vector2.up * R).Rotate((reverseOrder ? 1 : -1) * angle) + (Vector2.up * offset);

            if (num != colorCheck && colorCheck != numColors)
            {
                num++;
                continue;
            }

            float angleB = 0;
            for (float j = 0; j < 1; j += (1f / numColors))
            {
                if (j == 0 || j > .999f)
                    continue;

                angleB = (j + i) * 360;
                float hueB = (j + i) + startHue;
                float hueOverflowB = (int)((j + i) + startHue);
                hueB = hueB - hueOverflowB;
                Color cB = Color.HSVToRGB(hueB, 1, 1);

                //Color cM = c * cB;
                Color cM = mixMode switch
                {
                    MixMode.Multiply => c * cB,
                    MixMode.Add => (c + cB).Clamp01(),
                    _ => cB
                };

                Color.RGBToHSV(cM, out float h, out float s, out float v);
                float angleM = (h - startHue) * 360;
                //float RM = Mathf.Lerp(0, R, v);
                float RM = mixMode switch
                {
                    MixMode.Multiply => Mathf.Lerp(0, R, v),
                    MixMode.Add => Mathf.Lerp(0, R, s),
                    _ => R
                };

                Vector2 posM = (Vector2.up * RM).Rotate((reverseOrder ? 1 : -1) * angleM) + (Vector2.up * offset);
                Vector2 posB = (Vector2.up * R).Rotate((reverseOrder ? 1 : -1) * angleB) + (Vector2.up * offset);

                GLGizmos.SetColor(Color.black.SetAlpha(.75f));
                GLGizmos.DrawLine(pos, posM);
                GLGizmos.DrawLine(posM, posB);

                GLGizmos.SetColor(Color.black);
                GLGizmos.DrawSolidCircle(posM, rM * 1.1f);

                GLGizmos.SetColor(cM);
                GLGizmos.DrawSolidCircle(posM, rM);
            }

            num++;
        }
    }
}
