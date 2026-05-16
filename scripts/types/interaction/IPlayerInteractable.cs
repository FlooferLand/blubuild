using System.Threading.Tasks;

namespace Project;

public interface IPlayerInteractable {
    public ValueTask Interact(InteractContext ctx);
}