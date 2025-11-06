namespace EM.Domain.Interface
{
    public interface IEntidade
    {
        int GetHashCode();
        bool Equals(object obj);
        string ToString();
    }
}