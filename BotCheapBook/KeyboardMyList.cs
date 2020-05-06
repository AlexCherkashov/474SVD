using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;

namespace BotCheapBook
{
    class KeyboardMyList : IKeyboard
    {
        private MessageKeyboard keyboard;

        public KeyboardMyList()
        {
            var BooksStores = new string[3] { "Добавить", "Удалить", "Назад" };

            var buttons = new List<MessageKeyboardButton>();

            for (var i = 0; i < 3; i++)
            {
                buttons.Add(new MessageKeyboardButton
                {
                    Action = new MessageKeyboardButtonAction
                    {
                        Type = KeyboardButtonActionType.Text,
                        Label = BooksStores[i]
                    },
                    Color = KeyboardButtonColor.Positive
                });
            }
            buttons[2].Color = KeyboardButtonColor.Default;

            keyboard = new MessageKeyboard
            {
                OneTime = false,
                Buttons = new List<List<MessageKeyboardButton>>
            {
                new List<MessageKeyboardButton>
                {
                    buttons[0], buttons[1]
                },
                new List<MessageKeyboardButton>
                {
                    buttons[2]
                }
            }
            };
        }

        public string GetJsonKeyboard()
        {
            return JsonConvert.SerializeObject(keyboard);
        }
    }
}
