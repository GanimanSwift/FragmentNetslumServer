﻿using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static FragmentServerWV.Services.Extensions;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_MEMBERLIST)]
    public sealed class OPCODE_DATA_GUILD_MEMBERLIST : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var u = swap16(BitConverter.ToUInt16(request.Data, 0));
            if (u == 0)// Guild Member Category List
            {
                List<byte[]> listOfClasses = GuildManagementService.GetInstance().GetClassList();
                responses.Add(request.CreateResponse(0x7611, BitConverter.GetBytes(swap16((ushort)listOfClasses.Count))));
                foreach (var className in listOfClasses)
                {
                    responses.Add(request.CreateResponse(0x7613, className));
                }

            }
            else //MemberList in that Category
            {
                List<byte[]> memberList = GuildManagementService.GetInstance().GetGuildMembersListByClass(request.Client._guildID, u, request.Client._characterPlayerID);
                responses.Add(request.CreateResponse(0x7614, BitConverter.GetBytes(swap16((ushort)memberList.Count))));
                foreach (var member in memberList)
                {
                    responses.Add(request.CreateResponse(0x7615, member));
                }
            }
            return Task.FromResult<IEnumerable<ResponseContent>>(responses);
        }
    }
}
