using System;
using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IStyleRepository
{
    List<Style> GetAll();
    Style? Get(string id);
    void Create(Style style);
    void Update(Style style);
    void Delete(string id);
}
