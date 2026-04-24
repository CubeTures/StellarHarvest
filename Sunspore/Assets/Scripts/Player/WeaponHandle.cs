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
            SpriteRenderer _playerSr;
            public SpriteRenderer headSr;

            int _currentDirection = -1;
            int _sortOffset = 100; // default offset


            void Awake()
            {
                _anim = GetComponent<Animator>();
                _sr = GetComponent<SpriteRenderer>();
                // Go up to the parent transform directly, avoiding the current GameObject
                _playerSr = transform.parent.GetComponent<SpriteRenderer>();
                
                var headTransform = transform.Find("WeaponHead");
                if (headTransform != null)
                {
                    headSr = headTransform.GetComponent<SpriteRenderer>();
                }

                if (headSr == null)
                {
                    var renderers = GetComponentsInChildren<SpriteRenderer>();
                    foreach (var renderer in renderers)
                    {
                        if (renderer != _sr)
                        {
                            headSr = renderer;
                            break;
                        }
                    }
                }
            }

            void Update()
            {
                int playerSortOrder = _playerSr.sortingOrder;
                _sr.sortingOrder = playerSortOrder + 100; // base always in front
                headSr.sortingOrder = playerSortOrder + _sortOffset; // head varies by direction
            }

            public void SetWeaponBaseTransform(int dir, bool flipped)
            {
                _anim.SetInteger("Direction", dir);
                _currentDirection = dir;

                // Only the head changes depth based on direction
                _sortOffset = (dir == 3) ? -100 : 100;
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
        }
