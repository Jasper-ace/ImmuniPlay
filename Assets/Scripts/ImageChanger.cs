using UnityEngine;
using UnityEngine.UI;

public class ImageChanger : MonoBehaviour
{
    public Image displayImage;
    public Sprite[] images;

    private int currentImage = 0;

    void Start()
    {
        displayImage.sprite = images[currentImage];
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            NextImage();
        }
    }

    void NextImage()
    {
        currentImage++;

        if (currentImage < images.Length)
        {
            displayImage.sprite = images[currentImage];
        }
    }
}