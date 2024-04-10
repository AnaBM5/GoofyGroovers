using Microsoft.Xna.Framework;
using System;

public class BlobEntity
{
    public string blobUserName { get; set; }
    public int blobUserId { get; set; }
    public Color blobUserColor { get; set; }

    public Vector2 worldPosition { get; set; }

    public bool isJumping { get; set; } = false;
    public float jumpTheta { get; set; }
    public Vector2 jumpStartPoint { get; set; }
    public Vector2 jumpEndPoint { get; set; }

    public double finishTime { get; set; }


    public float velocity { get; set; }
    public float elapsedSecondsSinceJumpStart { get; set; }
    public bool isStartingTheRace { get; set; }

    public BlobEntity()
    { }

    public BlobEntity(string name, Color color, Vector2 position)
    {
        this.blobUserName = name;
        this.blobUserColor = color;
        this.worldPosition = position;
        Random random = new();
        blobUserId = random.Next(1000);
    }

    public BlobEntity(string name, bool isOwnedByUser, int blobUserId, Color blobUserColor, Vector2 position) : this(name, blobUserColor, position)
    {
        this.blobUserId = blobUserId;
    }

    public BlobEntity(string name, bool isOwnedByUser, int blobUserId, Color color, Vector2 position, bool isJumping, Vector2 jumpStartPoint, Vector2 jumpEndPoint) : this(name, isOwnedByUser, blobUserId, color, position)
    {
        this.isJumping = isJumping;
        this.jumpStartPoint = jumpStartPoint;
        this.jumpEndPoint = jumpEndPoint;
    }
}