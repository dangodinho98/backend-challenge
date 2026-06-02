using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

public class ExternalIdentity
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private ExternalIdentity()
    {
    }

    public ExternalIdentity(Guid id, string description)
    {
        if (id == Guid.Empty)
            throw new DomainException("External identity id cannot be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("External identity description is required.");

        Id = id;
        Description = description.Trim();
    }
}
