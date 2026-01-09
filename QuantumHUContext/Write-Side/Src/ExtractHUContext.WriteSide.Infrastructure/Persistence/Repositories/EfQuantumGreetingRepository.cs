using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Models.Snapshots;
using ExtractHUContext.WriteSide.Domain.Ports;
using ExtractHUContext.WriteSide.Infrastructure.Persistence.Entities;

namespace ExtractHUContext.WriteSide.Infrastructure.Persistence.Repositories;

public class EfQuantumGreetingRepository(QuantumHUDbContext dbContext) : IQuantumGreetingRepository
{
    public async Task SaveAsync(QuantumGreeting greeting)
    {
        var snapshot = greeting.ToSnapshot();

        var entity = await dbContext.QuantumGreetings.FindAsync(snapshot.Id);

        if (entity == null)
        {
            entity = new QuantumGreetingEf
            {
                Id = snapshot.Id,
                Message = snapshot.Message,
                CreatedAt = snapshot.CreatedAt
            };
            await dbContext.QuantumGreetings.AddAsync(entity);
        }
        else
        {
            entity.Message = snapshot.Message;
            entity.CreatedAt = snapshot.CreatedAt;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<QuantumGreeting?> GetByIdAsync(Guid id)
    {
        var entity = await dbContext.QuantumGreetings.FindAsync(id);
        if (entity == null)
            return null;

        var snapshot = new QuantumGreetingSnapshot(
            entity.Id,
            entity.Message,
            entity.CreatedAt
        );

        return QuantumGreeting.FromSnapshot(snapshot);
    }
}
