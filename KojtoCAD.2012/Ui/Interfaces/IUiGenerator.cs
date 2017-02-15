namespace KojtoCAD.Ui.Interfaces
{
    public interface IUiGenerator
    {
        void GenerateUi(bool regenerateIfExists);
        void RegenerateUi();

        void RemoveUi();
    }
}
