
namespace WarRegions.Core.Engine
{
    public class Transform
    {
        private readonly object _childrenLock = new object();
        private readonly List<Transform> _children = new List<Transform>();

        public GameObject gameObject { get; }
        public Transform parent { get; private set; }

        // Local space
        private Vector3 _localPosition = Vector3.zero;
        private Quaternion _localRotation = Quaternion.identity;
        private Vector3 _localScale = Vector3.one;

        public Vector3 localPosition
        {
            get => _localPosition;
            set => _localPosition = value;
        }

        public Quaternion localRotation
        {
            get => _localRotation;
            set => _localRotation = value;
        }

        public Vector3 localScale
        {
            get => _localScale;
            set => _localScale = value;
        }

        // World space
        public Vector3 position
        {
            get
            {
                if (parent == null) return localPosition;
                return parent.TransformPoint(localPosition);
            }
            set
            {
                if (parent == null) localPosition = value;
                else localPosition = parent.InverseTransformPoint(value);
            }
        }

        public Quaternion rotation
        {
            get
            {
                if (parent == null) return localRotation;
                return parent.rotation * localRotation;
            }
            set
            {
                if (parent == null) localRotation = value;
                else
                {
                    var invParent = parent.rotation.Inverse();
                    localRotation = invParent * value;
                }
            }
        }

        public Vector3 lossyScale
        {
            get
            {
                if (parent == null) return localScale;
                var ps = parent.lossyScale;
                return new Vector3(ps.x * localScale.x, ps.y * localScale.y, ps.z * localScale.z);
            }
        }

        public int childCount
        {
            get
            {
                lock (_childrenLock) { return _children.Count; }
            }
        }

        public Transform(GameObject owner)
        {
            gameObject = owner ?? throw new ArgumentNullException(nameof(owner));
            Debug.Log($"Transform created for GameObject: {owner.name}");
        }

        public void SetParent(Transform newParent, bool worldPositionStays = true)
        {
            if (newParent == parent) return;

            // compute new local values if we are keeping world position
            if (worldPositionStays)
            {
                var worldPos = position;
                var worldRot = rotation;
                var worldScale = lossyScale;

                // detach from old parent
                DetachFromParent();

                // attach
                AttachToParent(newParent);

                // restore world transform by computing local from new parent
                if (newParent == null)
                {
                    localPosition = worldPos;
                    localRotation = worldRot;
                    localScale = worldScale; // best effort
                }
                else
                {
                    localPosition = newParent.InverseTransformPoint(worldPos);
                    var inv = newParent.rotation.Inverse();
                    localRotation = inv * worldRot;
                    // convert scale approximately
                    var parentScale = newParent.lossyScale;
                    localScale = new Vector3(
                        parentScale.x != 0f ? worldScale.x / parentScale.x : worldScale.x,
                        parentScale.y != 0f ? worldScale.y / parentScale.y : worldScale.y,
                        parentScale.z != 0f ? worldScale.z / parentScale.z : worldScale.z
                    );
                }
            }
            else
            {
                DetachFromParent();
                AttachToParent(newParent);
            }
        }

        private void DetachFromParent()
        {
            if (parent == null) return;
            lock (parent._childrenLock)
            {
                parent._children.Remove(this);
            }
            parent = null;
        }

        private void AttachToParent(Transform newParent)
        {
            if (newParent == null)
            {
                parent = null;
                return;
            }

            parent = newParent;
            lock (newParent._childrenLock)
            {
                if (!newParent._children.Contains(this))
                    newParent._children.Add(this);
            }
        }

        public Transform GetChild(int index)
        {
            lock (_childrenLock)
            {
                if (index < 0 || index >= _children.Count) throw new ArgumentOutOfRangeException(nameof(index));
                return _children[index];
            }
        }

        public bool IsChildOf(Transform t)
        {
            var p = parent;
            while (p != null)
            {
                if (p == t) return true;
                p = p.parent;
            }
            return false;
        }

        public void AddChild(Transform child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            child.SetParent(this, worldPositionStays: false);
        }

        public void RemoveChild(Transform child)
        {
            if (child == null) return;
            if (child.parent != this) return;
            child.SetParent(null, worldPositionStays: false);
        }

        // Transform a point from local space to world space
        public Vector3 TransformPoint(Vector3 localPoint)
        {
            // apply local scale and rotation then translate by position
            var scaled = new Vector3(localPoint.x * localScale.x, localPoint.y * localScale.y, localPoint.z * localScale.z);
            var rotated = localRotation * scaled;
            var world = position + rotated;

            // if has parent, parent's TransformPoint already considered because position uses parent
            return world;
        }

        // Transform a direction (no translation)
        public Vector3 TransformDirection(Vector3 localDirection)
        {
            var rotated = localRotation * localDirection;
            // scale is not applied to directions in Unity's TransformDirection (it ignores scale)
            return rotated;
        }

        // Inverse transform: world point to local space
        public Vector3 InverseTransformPoint(Vector3 worldPoint)
        {
            var p = worldPoint - (parent == null ? Vector3.zero : parent.position);
            if (parent != null)
            {
                // bring to parent's local space first
                p = parent.InverseTransformPoint(worldPoint);
                // now remove parent's effect: but above already gives local to parent; we need this transform's local as:
                // local = inverse rotation * (world - parent.position) / parent.lossyScale ??? To keep it simple:
                // Compute world relative to this transform's world (i.e., local = inverse(rotation) * (world - position), then divide by localScale)
                var invRot = rotation.Inverse();
                var local = invRot * (worldPoint - position);
                return new Vector3(
                    local.x / localScale.x,
                    local.y / localScale.y,
                    local.z / localScale.z
                );
            }
            else
            {
                var invRot = localRotation.Inverse();
                var local = invRot * (worldPoint - localPosition);
                return new Vector3(
                    local.x / localScale.x,
                    local.y / localScale.y,
                    local.z / localScale.z
                );
            }
        }

        // Convenience operations
        public void Translate(Vector3 translation, Space relativeTo = Space.Self)
        {
            if (relativeTo == Space.Self)
            {
                localPosition += localRotation * translation;
            }
            else
            {
                position += translation;
            }
        }

        public void Rotate(Quaternion delta)
        {
            localRotation = localRotation * delta;
        }

        public void LookAt(Vector3 worldPoint)
        {
            var dir = (worldPoint - position).Normalized();
            if (dir.sqrMagnitude == 0f) return;
            // build a quaternion that looks along dir with up = Vector3.up
            localRotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        // Simple enums and helper types
        public enum Space { Self, World }

        // Minimal Vector3 implementation
        public struct Vector3
        {
            public float x, y, z;

            public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

            public static Vector3 zero => new Vector3(0f, 0f, 0f);
            public static Vector3 one => new Vector3(1f, 1f, 1f);
            public static Vector3 up => new Vector3(0f, 1f, 0f);
            public static Vector3 right => new Vector3(1f, 0f, 0f);
            public static Vector3 forward => new Vector3(0f, 0f, 1f);

            public float sqrMagnitude => x * x + y * y + z * z;
            public float magnitude => (float)Math.Sqrt(sqrMagnitude);

            public Vector3 Normalized()
            {
                var m = magnitude;
                if (m == 0f) return zero;
                return new Vector3(x / m, y / m, z / m);
            }

            public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
            public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
            public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
            public static Vector3 operator *(float d, Vector3 a) => a * d;
            public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.x / d, a.y / d, a.z / d);

            public override string ToString() => $"({x:F3}, {y:F3}, {z:F3})";
        }

        // Minimal Quaternion implementation (sufficient for basic rotation math)
        public struct Quaternion
        {
            public float x, y, z, w;

            public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }

            public static Quaternion identity => new Quaternion(0f, 0f, 0f, 1f);

            public static Quaternion Euler(float xDeg, float yDeg, float zDeg)
            {
                // Convert degrees to radians and build from Euler (ZXY convention is common; this uses Yaw (y), Pitch (x), Roll (z))
                var cx = (float)Math.Cos(xDeg * Math.PI / 360.0);
                var sx = (float)Math.Sin(xDeg * Math.PI / 360.0);
                var cy = (float)Math.Cos(yDeg * Math.PI / 360.0);
                var sy = (float)Math.Sin(yDeg * Math.PI / 360.0);
                var cz = (float)Math.Cos(zDeg * Math.PI / 360.0);
                var sz = (float)Math.Sin(zDeg * Math.PI / 360.0);

                // Quaternion multiplication of yaw * pitch * roll
                var qx = new Quaternion(sx, 0f, 0f, cx);
                var qy = new Quaternion(0f, sy, 0f, cy);
                var qz = new Quaternion(0f, 0f, sz, cz);

                return qy * qx * qz;
            }

            public Quaternion Inverse()
            {
                // For unit quaternions, inverse is conjugate
                var norm = x * x + y * y + z * z + w * w;
                if (norm > 0f)
                {
                    var invNorm = 1f / norm;
                    return new Quaternion(-x * invNorm, -y * invNorm, -z * invNorm, w * invNorm);
                }
                return identity;
            }

            public static Quaternion operator *(Quaternion a, Quaternion b)
            {
                return new Quaternion(
                    a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
                    a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
                    a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
                    a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z
                );
            }

            // Rotate vector by quaternion: q * v * q^-1
            public static Vector3 operator *(Quaternion q, Vector3 v)
            {
                // Extract the vector part of the quaternion
                var u = new Vector3(q.x, q.y, q.z);
                var s = q.w;

                // v' = 2.0f * dot(u, v) * u
                //      + (s*s - dot(u, u)) * v
                //      + 2.0f * s * cross(u, v)
                var dot = u.x * v.x + u.y * v.y + u.z * v.z;
                var cross = new Vector3(
                    u.y * v.z - u.z * v.y,
                    u.z * v.x - u.x * v.z,
                    u.x * v.y - u.y * v.x
                );

                var term1 = u * (2f * dot);
                var term2 = v * (s * s - (u.x * u.x + u.y * u.y + u.z * u.z));
                var term3 = cross * (2f * s);

                return term1 + term2 + term3;
            }
            // أضف في نهاية Transform.cs قبل آخر }

    public struct Vector2
    {
        public float x, y;

        public Vector2(float x, float y) { this.x = x; this.y = y; }

        public static Vector2 zero => new Vector2(0f, 0f);
        public static Vector2 one => new Vector2(1f, 1f);
        public static Vector2 up => new Vector2(0f, 1f);
        public static Vector2 right => new Vector2(1f, 0f);

        public float sqrMagnitude => x * x + y * y;
        public float magnitude => (float)Math.Sqrt(sqrMagnitude);

        public Vector2 Normalized()
        {
            var m = magnitude;
            if (m == 0f) return zero;
            return new Vector2(x / m, y / m);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator *(Vector2 a, float d) => new Vector2(a.x * d, a.y * d);
        public static Vector2 operator *(float d, Vector2 a) => a * d;
        public static Vector2 operator /(Vector2 a, float d) => new Vector2(a.x / d, a.y / d);

        public override string ToString() => $"({x:F3}, {y:F3})";
        
        // إضافة خصائص X, Y للتوافق مع الكود الحالي
        public float X => x;
        public float Y => y;
    }

            public static Quaternion LookRotation(Vector3 forward, Vector3 up)
            {
                // Very simple implementation to create a quaternion looking along forward with given up
                var f = forward.Normalized();
                var u = up.Normalized();
                // compute orthonormal basis
                var r = new Vector3(
                    u.y * f.z - u.z * f.y,
                    u.z * f.x - u.x * f.z,
                    u.x * f.y - u.y * f.x
                ).Normalized();

                // construct rotation matrix columns (r, u', f)
                // Convert rotation matrix to quaternion (standard algorithm)
                float m00 = r.x, m01 = u.x, m02 = f.x;
                float m10 = r.y, m11 = u.y, m12 = f.y;
                float m20 = r.z, m21 = u.z, m22 = f.z;

                float trace = m00 + m11 + m22;
                if (trace > 0f)
                {
                    float s = (float)Math.Sqrt(trace + 1f) * 2f;
                    return new Quaternion((m21 - m12) / s, (m02 - m20) / s, (m10 - m01) / s, 0.25f * s);
                }
                if (m00 > m11 && m00 > m22)
                {
                    float s = (float)Math.Sqrt(1f + m00 - m11 - m22) * 2f;
                    return new Quaternion(0.25f * s, (m01 + m10) / s, (m02 + m20) / s, (m21 - m12) / s);
                }
                if (m11 > m22)
                {
                    float s = (float)Math.Sqrt(1f + m11 - m00 - m22) * 2f;
                    return new Quaternion((m01 + m10) / s, 0.25f * s, (m12 + m21) / s, (m02 - m20) / s);
                }
                else
                {
                    float s = (float)Math.Sqrt(1f + m22 - m00 - m11) * 2f;
                    return new Quaternion((m02 + m20) / s, (m12 + m21) / s, 0.25f * s, (m10 - m01) / s);
                }
            }

            public override string ToString() => $"({x:F3}, {y:F3}, {z:F3}, {w:F3})";
        }
    }
}


/*
            public struct Vector2
            {
                public float x, y;
    
                public Vector2(float x, float y) { this.x = x; this.y = y; }
    
                public static Vector2 zero => new Vector2(0f, 0f);
                public static Vector2 one => new Vector2(1f, 1f);
    
                public float magnitude => (float)Math.Sqrt(x * x + y * y);
    
                public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
                public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
    
                public override string ToString() => $"({x}, {y})";
            }
*/