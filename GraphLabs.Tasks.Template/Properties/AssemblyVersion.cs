/* 
    ���� ���� ������������ ������������� ��� ������ �������.
    ����� ������ ������� � "version.txt" � ������� X.Y
   
    ������ ��� ����� ������������� �� ������, ������ ���� ��������� �������:
    * � ������� ����� ����������� ����������������� ���������
    * ������ ������������ �� ����� master
    * ������ ������������ � ������������ Release
*/

using System.Reflection;

#if DEBUG
[assembly: AssemblyVersion("0.0.27.*")]
#else
[assembly: AssemblyVersion("1.0.27")]
#endif