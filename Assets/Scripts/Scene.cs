// COMP30019 - Graphics and Interaction
// (c) University of Melbourne, 2022

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Scene : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] [Range(1, 179)] private float fieldOfView = 60f;
    
    [SerializeField] private LinesGenerator debug;

    private float _imagePlaneHeight;
    private float _imagePlaneWidth;
    
    private void Start()
    {
        // Figure out how the image is scaled in the world (the image "plane").
        ComputeWorldImageBounds();
        
        // Place the image in the world accordingly, so we can visualise this.
        EmbedImageInWorld();

        // Perform ray tracing to render the image.
        Render();
        
        // Add additional visualisations to help debug things.
        DebugVisualisations();
    }

    private void DebugVisualisations()
    {
        // Here you may use "debug rays" to visualise rays in the scene.

        // Image plane "corner" rays first (frustum edges).
        this.debug.Ray(new Ray(Vector3.zero, NormalizedImageToWorldCoord(0f, 0f)), Color.blue);

        // Add more rays to visualise here...
        this.debug.Ray(new Ray(Vector3.zero, NormalizedImageToWorldCoord(0f, 1f)), Color.blue);
        this.debug.Ray(new Ray(Vector3.zero, NormalizedImageToWorldCoord(1f, 0f)), Color.blue);
        this.debug.Ray(new Ray(Vector3.zero, NormalizedImageToWorldCoord(1f, 1f)), Color.blue);

        // fire debug rays
        for (int x = 0; x < this.image.Width; x++)
        {
            for (int y = 0; y < this.image.Height; y++)
            {
                this.debug.Ray(fireNormRay(x, y), Color.white);
            }
        }
        
    }

    private Ray fireNormRay(int x, int y)
    {
        var normX = (x + 0.5f) / this.image.Width;
        var normY = (y + 0.5f) / this.image.Height;
        var normRay = NormalizedImageToWorldCoord(normX, normY);
        return new Ray(Vector3.zero, normRay);
    }

    private void Render()
    {
        // Render the image here...
        for (int x = 0; x < this.image.Width; x++)
        {
            for (int y = 0; y < this.image.Height; y++)
            {
                // fallback black (no intersection)
                this.image.SetPixel(x, y, Color.black);

                // loop through all entities and find intersection with the ray
                foreach (var sceneEntity in FindObjectsOfType<SceneEntity>())
                {
                    if (sceneEntity.Intersect(fireNormRay(x, y)) != null)
                    {
                        this.image.SetPixel(x, y, sceneEntity.Color());
                    }
                }
               
            }
        }
    }

    private Vector3 NormalizedImageToWorldCoord(float x, float y)
    {
        return new Vector3(
            this._imagePlaneWidth * (x - 0.5f),
            this._imagePlaneHeight * (0.5f - y),
            1.0f); // Image plane is 1 unit from camera.
    }

    private void ComputeWorldImageBounds()
    {
        var aspectRatio = (float)this.image.Width / this.image.Height;
        var fovLength = Mathf.Tan(this.fieldOfView / 2f * Mathf.Deg2Rad) * 2f;

        // Note: We are using vertical FOV here.
        this._imagePlaneHeight = fovLength;
        this._imagePlaneWidth = this._imagePlaneHeight * aspectRatio;
    }

    private void EmbedImageInWorld()
    {
        this.image.transform.position = new Vector3(0f, 0f, 1f);
        this.image.transform.localScale = new Vector3(this._imagePlaneWidth, this._imagePlaneHeight, 0f);
    }
}
