﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spine;

namespace SuperFantasticSteampunk
{
    class Entity
    {
        #region Instance Properties
        public Skeleton Skeleton { get; private set; }
        public Sprite Sprite { get; private set; }
        public AnimationState AnimationState { get; private set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Scale { get; set; }
        public float Rotation { get; set; }
        public float AngularVelocity { get; set; }
        public Color Tint { get; set; }
        #endregion

        #region Constructors
        public Entity(Skeleton skeleton, Vector2 position)
            : this((Sprite)null, position)
        {
            Skeleton = skeleton;
            AnimationState = new AnimationState(new AnimationStateData(skeleton.Data));
        }

        public Entity(Sprite sprite, Vector2 position)
        {
            ResetManipulation();
            this.Sprite = sprite;
            Position = position;
        }
        #endregion

        #region Instance Methods
        public void ResetManipulation(List<string> exclude = null)
        {
            if (exclude == null)
                exclude = new List<string>();

            if (!exclude.Contains("Position"))
                Position = new Vector2(0.0f);
            if (!exclude.Contains("Velocity"))
                Velocity = new Vector2(0.0f);
            if (!exclude.Contains("Scale"))
                Scale = new Vector2(1.0f);
            if (!exclude.Contains("Rotation"))
                Rotation = 0.0f;
            if (!exclude.Contains("AngularVelocity"))
                AngularVelocity = 0.0f;
            if (!exclude.Contains("Tint"))
                Tint = Color.White;
        }

        public void SetSkeletonAttachment(string slotName, string attachmentName, TextureData textureData = null, bool forceNoTextureData = false)
        {
            if (Skeleton == null || Skeleton.FindSlot(slotName) == null)
                return;
            
            if (Skeleton.GetAttachment(slotName, attachmentName) == null)
            {
                if (textureData != null)
                {
                    addSkeletonAttachment(slotName, attachmentName, textureData);
                    Skeleton.SetAttachment(slotName, attachmentName);
                }
                else if (forceNoTextureData)
                {
                    Skeleton.Data.FindSkin("default").AddAttachment(Skeleton.FindSlotIndex(slotName), attachmentName, new RegionAttachment(attachmentName));
                    Skeleton.SetAttachment(slotName, attachmentName);
                }
            }
            else
                Skeleton.SetAttachment(slotName, attachmentName);
        }

        public bool CollidesWith(Entity other)
        {
            if (Sprite == null || other.Sprite == null)
                return false;
            return GetBoundingBox().Intersects(other.GetBoundingBox());
        }

        public Rectangle GetBoundingBox()
        {
            return new Rectangle(
                (int)(Position.X - (Sprite.Data.OriginX * Scale.X)),
                (int)(Position.Y - (Sprite.Data.OriginY * Scale.Y)),
                (int)(Sprite.Data.Width * Scale.X),
                (int)(Sprite.Data.Height * Scale.Y)
            );
        }

        public virtual void Kill()
        {
            Scene.RemoveEntity(this);
        }

        public virtual void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Rotation += AngularVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Skeleton != null)
                updateSkeleton(gameTime);
            else
                Sprite.Update(gameTime);
        }

        public virtual void Draw(Renderer renderer)
        {
            if (Skeleton != null)
                renderer.Draw(Skeleton);
            else
                renderer.Draw(Sprite, Position, Tint, Rotation, Scale);
        }

        private void updateSkeleton(GameTime gameTime)
        {
            Skeleton.RootBone.X = Position.X;
            Skeleton.RootBone.Y = Position.Y;
            Skeleton.RootBone.ScaleX = Scale.X;
            Skeleton.RootBone.ScaleY = Scale.Y;
            Skeleton.RootBone.Rotation = Rotation;
            AnimationState.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            AnimationState.Apply(Skeleton);
            Skeleton.UpdateWorldTransform();
        }

        private void addSkeletonAttachment(string slotName, string attachmentName, TextureData textureData)
        {
            RegionAttachment regionAttachment = new RegionAttachment(attachmentName);

            regionAttachment.RendererObject = textureData.Texture;
            regionAttachment.Width = regionAttachment.RegionWidth = regionAttachment.RegionOriginalWidth = textureData.Texture.Width;
            regionAttachment.Height = regionAttachment.RegionHeight = regionAttachment.RegionOriginalHeight = textureData.Texture.Height;
            regionAttachment.RegionOffsetX = textureData.OriginX;
            regionAttachment.RegionOffsetY = textureData.OriginY;
            regionAttachment.Rotation = textureData.Rotation;
            regionAttachment.SetUVs(0, 0, 1, 1, false);
            regionAttachment.UpdateOffset();

            Skeleton.Data.FindSkin("default").AddAttachment(Skeleton.FindSlotIndex(slotName), attachmentName, regionAttachment);
        }
        #endregion
    }
}
