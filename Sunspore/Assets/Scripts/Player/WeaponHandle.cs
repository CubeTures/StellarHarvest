using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Sunspore;
using UnityEngine;

        public class WeaponHandle : MonoBehaviour
        {
            HandHeld.WeaponHandle handle;
            Animator _anim;
            SpriteRenderer _sr;
            public SpriteRenderer headSr;

            int _currentDirection = -1;
            int sortOrder = 0;

            
            void Awake()
            {
                _anim = GetComponent<Animator>();
                _sr = GetComponent<SpriteRenderer>();
            }

            public void TypeUpdate(HandHeld.WeaponHandle newHandle)
            {
                handle = newHandle;
                _anim.SetInteger("Type", (int)handle);
            }

            public void SetHeadSprite(Sprite sprite)
            {
                if (headSr != null)
                {
                    headSr.sprite = sprite;
                }
            }
            public void Swing()
            {
                _anim.SetInteger("Direction", _currentDirection);
                _anim.SetTrigger("Swing");
            }
            public void SetWeaponBaseTransform(int dir, bool flipped)
            {
                /*_sr.flipX = flipped;
                _head.flipX = flipped;*/

                _anim.SetInteger("Direction", dir);
                _currentDirection = dir;
            }
        }
