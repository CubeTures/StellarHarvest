    using UnityEngine;

        public class WeaponBase : MonoBehaviour
        {
            public enum WeaponBaseType {WOOD = 1}
            Animator _anim;
            SpriteRenderer _sr;
            void Start()
            {
                _anim = GetComponent<Animator>();
                _sr = GetComponent<SpriteRenderer>();
            }
            public void Swing(WeaponBaseType type)
            {
                _anim.SetTrigger("Swing");
                switch(type)
                {
                    case WeaponBaseType.WOOD:
                    _anim.SetInteger("Type", (int)type);
                    break;

                    default:
                    _anim.SetInteger("Type", 1);
                    break;
                }
            }
            public void SetWeaponBaseTransform(int dir, bool flipped)
            {
                switch(dir)
                {
                    case 1: //front
                    _sr.flipX = !flipped;
                    _anim.SetInteger("Direction", dir);
                    break;

                    case 2: //right
                    _sr.flipX = true;
                    _anim.SetInteger("Direction", dir);
                    break;

                    case 3: //back
                    _sr.flipX = flipped;
                    _anim.SetInteger("Direction", dir);
                    break;

                    case 4: //left
                    _sr.flipX = false;
                    _anim.SetInteger("Direction", dir);
                    break;
                }
            }
        }
