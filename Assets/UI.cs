using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{

    public int mass, density, viscosity;
    public float radius;

    public Text massValue, densityValue, viscosityValue, radiusValue;

    public Slider massSlider, densitySlider, viscositySlider, radiusSlider;

    // Start is called before the first frame update
    void Start()
    {
        mass = (int)massSlider.value;
        density = (int)densitySlider.value;
        viscosity = (int)viscositySlider.value;
        radius = radiusSlider.value;

        massValue.text = mass.ToString();
        densityValue.text = density.ToString();
        viscosityValue.text = viscosity.ToString();
        radiusValue.text = radius.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        mass = (int)massSlider.value;
        density = (int)densitySlider.value;
        viscosity = (int)viscositySlider.value;
        radius = radiusSlider.value;

        massValue.text = mass.ToString();
        densityValue.text = density.ToString();
        viscosityValue.text = viscosity.ToString();
        radiusValue.text = radius.ToString();
    }
}
