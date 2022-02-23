﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Xbim.InformationSpecifications
{
    public enum Container
    {
        Undefined,
        IfcElementAssembly,
        IfcGroup,
        IfcSystem
    }

    // todo: IDSTALK: partof asymmetry
    //
    // why is it that partof can only be in the requirement side of a specification?
    // I can see the need to have a certain property/classification for anything belonging to a system
     
    // todo: IDSTALK: instuctions is missing only on partof facet
    // 
    public class PartOfFacet : IFacet, IEquatable<PartOfFacet>
    {
        public string Entity { get; set; } = string.Empty;

        Container GetEntity()
        {
            if (Enum.TryParse<Container>(Entity, out var loc))
            {
                return loc;
            }
            return Container.Undefined;
        }

        void SetEntity(Container value)
        {
            Entity = value.ToString();
        }


        public bool Equals(PartOfFacet other)
        {
            if (other == null)
                return false;
            var thisEqual = (Entity, true)
                .Equals((other.Entity, true));
            return thisEqual;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as PartOfFacet);
        }

        public bool IsValid()
        {
            return GetEntity() != Container.Undefined;
        }

        public string Short()
        {
            if (IsValid())
                return $"belongs to {Entity}";
            return "belongs to undefined group";
        }

        public override string ToString()
        {
            return $"belongs to :'{Entity}'";
        }

        public override int GetHashCode() => (Entity, true).GetHashCode();
    }
}
