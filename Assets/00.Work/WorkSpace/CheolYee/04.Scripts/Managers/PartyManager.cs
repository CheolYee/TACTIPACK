using System.Collections.Generic;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class PartyManager : MonoSingleton<PartyManager>
    {
        [SerializeField] private List<Player> players = new();
        public List<Player> Players => players;
    }
}