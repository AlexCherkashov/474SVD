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
    class KeyboardSites : IKeyboard
    {
        private MessageKeyboard keyboard;

        public KeyboardSites()
        {
            var BooksStores = new string[6] { "Лабиринт", "book24", "my-shop", "Дом Книги", "Республика", "Назад" };

            var buttons = new List<MessageKeyboardButton>();

            for (var i = 0; i < 6; i++)
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

            buttons[5].Color = KeyboardButtonColor.Default;

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
                    buttons[2], buttons[3]
                },
                new List<MessageKeyboardButton>
                {
                    buttons[4], buttons[5]
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
