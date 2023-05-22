using UnityEngine;

public class Math : MonoBehaviour
{
    /// <summary>
    /// ��ˮƽ��ת�Ƕ�ת��Ϊˮƽ����
    /// </summary>
    /// <param name="Hrot">����Ϊ�Ƕ���</param>
    public static Vector3 getHVector(float Hrot)
    {//����Ϊ�Ƕ���
        Hrot *= Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(Hrot), 0, Mathf.Cos(Hrot)).normalized;
    }

    /// <summary>
    /// ��ˮƽ����ת��Ϊˮƽ��ת�Ƕ�
    /// </summary>
    /// <returns>���ؽǶ���</returns>
    public static float getHRotation(Vector3 dir)
    {//���Ϊ�Ƕ��ƣ���0��90
        float ag = Mathf.Atan(dir.x / dir.z) * Mathf.Rad2Deg;
        ag = (ag % 180 + 180) % 180;
        if (dir.x < 0) ag += 180;
        if (dir.x == 0) ag = dir.z > 0 ? 0 : 180;
        return ag;
    }

}
