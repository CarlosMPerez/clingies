using System;
using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IStyleRepository
{
    List<Style> GetAll();
    List<Style> GetAllActive();
    Style? Get(string id);
    Style GetDefault();
    void Create(Style style);
    void Update(Style style);
    void Delete(string id);
    void MarkActive(string id, bool isActive);
    void MarkDefault(string id, bool isDefault);
}
