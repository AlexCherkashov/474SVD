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
    class KeyboardBestSellers : IKeyboard
    {
        private MessageKeyboard keyboard;

        public KeyboardBestSellers()
        {
            var BooksStores = new string[5] { "Лабиринт", "book24", "Дом Книги", "Республика", "Назад" };

            var buttons = new List<MessageKeyboardButton>();

            for (var i = 0; i < 5; i++)
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
            buttons[4].Color = KeyboardButtonColor.Default;

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
                    buttons[4]
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
