﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.IRC.Messages
{
    public class Notice : ReceivedMessage
    {
        /// <summary>
        /// <see href="https://dev.twitch.tv/docs/irc/msg-id/"/>
        /// </summary>
        public NoticeId? MessageId => Enum.TryParse(TryGetTag("msg-id"), out NoticeId value)
            ? value : null;
        public string TargetUserId => TryGetTag("target-user-id");

        public Notice(ReceivedMessage message) : base(message)
        {
            Debug.Assert(MessageType == IrcMessageType.NOTICE,
                $"{nameof(Notice)} must have type {IrcMessageType.NOTICE}" +
                $" but has {MessageType}");
        }
    }
    /// <summary>
    /// <see href="https://dev.twitch.tv/docs/irc/msg-id/"/>
    /// </summary>
    public enum NoticeId
    {
        already_banned,
        already_emote_only_off,
        already_emote_only_on,
        already_followers_off,
        already_followers_on,
        already_r9k_off,
        already_r9k_on,
        already_slow_off,
        already_slow_on,
        already_subs_off,
        already_subs_on,
        autohost_receive,
        bad_ban_admin,
        bad_ban_anon,
        bad_ban_broadcaster,
        bad_ban_mod,
        bad_ban_self,
        bad_ban_staff,
        bad_commercial_error,
        bad_delete_message_broadcaster,
        bad_delete_message_mod,
        bad_host_error,
        bad_host_hosting,
        bad_host_rate_exceeded,
        bad_host_rejected,
        bad_host_self,
        bad_mod_banned,
        bad_mod_mod,
        bad_slow_duration,
        bad_timeout_admin,
        bad_timeout_anon,
        bad_timeout_broadcaster,
        bad_timeout_duration,
        bad_timeout_mod,
        bad_timeout_self,
        bad_timeout_staff,
        bad_unban_no_ban,
        bad_unhost_error,
        bad_unmod_mod,
        bad_vip_grantee_banned,
        bad_vip_grantee_already_vip,
        bad_vip_max_vips_reached,
        bad_vip_achievement_incomplete,
        bad_unvip_grantee_not_vip,
        ban_success,
        cmds_available,
        color_changed,
        commercial_success,
        delete_message_success,
        delete_staff_message_success,
        emote_only_off,
        emote_only_on,
        followers_off,
        followers_on,
        followers_on_zero,
        host_off,
        host_on,
        host_receive,
        host_receive_no_count,
        host_target_went_offline,
        hosts_remaining,
        invalid_user,
        mod_success,
        msg_banned,
        msg_bad_characters,
        msg_channel_blocked,
        msg_channel_suspended,
        msg_duplicate,
        msg_emoteonly,
        msg_followersonly,
        msg_followersonly_followed,
        msg_followersonly_zero,
        msg_r9k,
        msg_ratelimit,
        msg_rejected,
        msg_rejected_mandatory,
        msg_requires_verified_phone_number,
        msg_slowmode,
        msg_subsonly,
        msg_suspended,
        msg_timedout,
        msg_verified_email,
        no_help,
        no_mods,
        no_vips,
        not_hosting,
        no_permission,
        r9k_off,
        r9k_on,
        raid_error_already_raiding,
        raid_error_forbidden,
        raid_error_self,
        raid_error_too_many_viewers,
        raid_error_unexpected,
        raid_notice_mature,
        raid_notice_restricted_chat,
        room_mods,
        slow_off,
        slow_on,
        subs_off,
        subs_on,
        timeout_no_timeout,
        timeout_success,
        tos_ban,
        turbo_only_color,
        unavailable_command,
        unban_success,
        unmod_success,
        unraid_error_no_active_raid,
        unraid_error_unexpected,
        unraid_success,
        unrecognized_cmd,
        untimeout_banned,
        untimeout_success,
        unvip_success,
        usage_ban,
        usage_clear,
        usage_color,
        usage_commercial,
        usage_disconnect,
        usage_delete,
        usage_emote_only_off,
        usage_emote_only_on,
        usage_followers_off,
        usage_followers_on,
        usage_help,
        usage_host,
        usage_marker,
        usage_me,
        usage_mod,
    }
}
