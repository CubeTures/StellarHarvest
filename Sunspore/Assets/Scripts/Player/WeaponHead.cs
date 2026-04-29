using System;
using Sunspore;
using UnityEngine;

        public class WeaponHead : MonoBehaviour
        {
            Animator _anim;
            SpriteRenderer _sr;
            HandHeld.WeaponHead head;
            Player player;
            int sortOrder = 0;
            private enum SlashEffect { METAL = 1 };


            void Awake()
            {
                _anim = GetComponent<Animator>();
                _sr = GetComponent<SpriteRenderer>();
                if (_sr == null)
                {
                    _sr = GetComponentInChildren<SpriteRenderer>();
                }
                player = GetComponentInParent<Player>();
                
                SetSwingTransform(1, false);
            }

            void Update()
            {
                sortOrder = player.spriteRenderer.sortingOrder;
            }
            public void TypeUpdate(HandHeld.WeaponHead newHead, HandHeld handheld)
            {
                head = newHead;
                if (handheld != null && handheld.headSprite != null)
                {
                    _sr.sprite = handheld.headSprite;
                }

                switch(head)
                {
                    case HandHeld.WeaponHead.SHOVEL:
                        _anim.SetInteger("Type", (int)SlashEffect.METAL);
                        // Debug.Log("Shovel equipped");
                        break;

                    case HandHeld.WeaponHead.HOE:
                        _anim.SetInteger("Type", (int)SlashEffect.METAL);
                        // Debug.Log("Hoe equipped");
                        break;

                    case HandHeld.WeaponHead.SCYTHE:
                        _anim.SetInteger("Type", (int)SlashEffect.METAL);
                        // Debug.Log("Scythe equipped");
                        break;

                    default:
                        break;
                }
            }
            public void Swing()
            {
                _anim.SetTrigger("Swing");
            }
            public void SetSwingTransform(int dir, bool flipped)
            {
                switch(dir)
                {
                    case 1: //front
                    transform.localPosition = new Vector3(0, -0.4f, -1);
                    transform.localRotation = Quaternion.Euler(-90f, 0, 180f);
                    _sr.flipX = !flipped;
                    sortOrder += 100;
                    break;

                    case 2: //right
                    transform.localPosition = new Vector3(1, -0.4f, 0);
                    transform.localRotation = Quaternion.Euler(-90f, 0, 90);
                    _sr.flipX = true;
                    sortOrder += 100;

                    break;
    
                    case 3: //back
                    transform.localPosition = new Vector3(0, -0.4f, 1);
                    transform.localRotation = Quaternion.Euler(-90, 0, 0);
                    _sr.flipX = flipped;
                    sortOrder -= 100;
                    break;

                    case 4: //left
                    transform.localPosition = new Vector3(-1, -0.4f, 0);
                    transform.localRotation = Quaternion.Euler(-90, 0, -90);
                    _sr.flipX = false;
                    sortOrder += 100;
                    break;
                }
            }
        }
