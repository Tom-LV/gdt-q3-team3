using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalCube : MonoBehaviour
{
    [Header("Scene refrences")]
    public SequencePuzzleManager sequencePuzzleManager;

    [Header("Face Configuration")]
    public Element TopFace = Element.Fire;       // Local Vector3.up
    public Element BottomFace = Element.Water;   // Local Vector3.down
    public Element FrontFace = Element.Light;    // Local Vector3.forward
    public Element BackFace = Element.Darkness;  // Local Vector3.back
    public Element RightFace = Element.Air;      // Local Vector3.right
    public Element LeftFace = Element.Earth;     // Local Vector3.left

    public float tileSize;

    private bool _isRolling = false;
    private Dictionary<Vector3, Element> _faceMap;

    private void Start()
    {
        _faceMap = new Dictionary<Vector3, Element>
        {
            { Vector3.up, TopFace },
            { Vector3.down, BottomFace },
            { Vector3.forward, FrontFace },
            { Vector3.back, BackFace },
            { Vector3.right, RightFace },
            { Vector3.left, LeftFace }
        };
    }

    public void MoveForward()
    {
        TryRoll(Vector3.forward);
    }

    public void MoveBackwards()
    {
        TryRoll(Vector3.back);
    }

    public void MoveRight()
    {
        TryRoll(Vector3.right);
    }

    public void MoveLeft()
    {
        TryRoll(Vector3.left);
    }

    private void TryRoll(Vector3 direction)
    {
        if (_isRolling) return;
        Vector3 targetPosition = transform.position + direction * tileSize;

        // Get which element will land face-down
        Element predictedLandingElement = GetElementFacingDirection(direction);

        // Check what tile is at the target position using a Raycast
        if (Physics.Raycast(targetPosition + Vector3.up * 0f, Vector3.down, out RaycastHit hit, 10f))
        {
            PuzzleTile tile = hit.collider.GetComponent<PuzzleTile>();

            if (tile != null)
            {
                // Validation
                if (tile.RequiredElement == Element.None || tile.RequiredElement == predictedLandingElement)
                {
                    if (tile.RequiredElement != Element.None)
                    {
                        sequencePuzzleManager.OnNodeTouched(tile);
                    }
                    StartCoroutine(RollCube(direction));
                }
                else
                {
                    Debug.Log($"Movement Blocked! Tile requires {tile.RequiredElement}, but you are landing on {predictedLandingElement}.");
                }
            }
        }
        else
        {
            Debug.Log("Cannot move into the abyss!");
        }
    }

    private Element GetElementFacingDirection(Vector3 moveDirection)
    {
        float maxDot = -Mathf.Infinity;
        Element bestMatch = Element.None;

        foreach (var face in _faceMap)
        {
            // Convert the local face direction to a world direction
            Vector3 worldFaceDir = transform.TransformDirection(face.Key);

            // Compare it to our movement direction
            float dot = Vector3.Dot(worldFaceDir, moveDirection);

            // The face with a dot product closest to 1 is pointing in that direction
            if (dot > maxDot)
            {
                maxDot = dot;
                bestMatch = face.Value;
            }
        }

        return bestMatch;
    }

    private IEnumerator RollCube(Vector3 direction)
    {
        _isRolling = true;

        Vector3 targetPosition = transform.position + direction * tileSize;

        // Calculate the bottom edge pivot point
        Vector3 pivot = transform.position + (direction * tileSize * 0.5f) + (Vector3.down * tileSize * 0.5f);
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);

        float rollDuration = 0.3f;
        float elapsed = 0f;
        float totalRotation = 0f;

        while (elapsed < rollDuration)
        {
            float step = (90f / rollDuration) * Time.deltaTime;

            // Ensure we don't over-rotate
            if (totalRotation + step > 90f) step = 90f - totalRotation;

            transform.RotateAround(pivot, rotationAxis, step);

            totalRotation += step;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap precisely to the grid
        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(
            Mathf.Round(transform.eulerAngles.x / 90) * 90,
            Mathf.Round(transform.eulerAngles.y / 90) * 90,
            Mathf.Round(transform.eulerAngles.z / 90) * 90
        );

        _isRolling = false;
    }
}