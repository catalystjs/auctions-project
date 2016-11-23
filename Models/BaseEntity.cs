using System;

namespace beltexam.Models
{
    // Base Entity for all Models
    public abstract class BaseEntity
    {
        // Only UpdatedAt is here as CreatedAt should be housed in the model
        public DateTime UpdatedAt { get; set; }
    }
}