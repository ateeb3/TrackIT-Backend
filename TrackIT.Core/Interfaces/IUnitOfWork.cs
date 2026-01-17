using System;
using System.Collections.Generic;
using System.Text;
using TrackIT.Core.Entities;

namespace TrackIT.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> CompleteAsync();
    }
}
